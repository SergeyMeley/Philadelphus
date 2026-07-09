using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages
{
    /// <summary>
    /// Хранилище данных  
    /// </summary>
    public class DataStorageModel : IDataStorageModel, IDisposable
    {
        /// <summary>
        /// Уникальный идентификатор основного хранилища.
        /// </summary>
        public static Guid MainDataStorageUuid { get => Guid.Parse("00000000-0000-0000-0000-19201518a07e"); }

        private readonly ILogger _logger;
        private System.Timers.Timer? _timer;

        private bool _isAutoChecking;

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        private string _name = string.Empty;

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (IsMainDataStorage)
                    return;

                _name = value;
            }
        }

        /// <summary>
        /// Описание
        /// </summary>
        private string _description = string.Empty;

        /// <summary>
        /// Описание
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (IsMainDataStorage)
                    return;

                _description = value;
            }
        }

        /// <summary>
        /// Тип хранилища данных
        /// </summary>
        private InfrastructureTypes _infrastructureType;

        /// <summary>
        /// Тип хранилища данных
        /// </summary>
        public InfrastructureTypes InfrastructureType
        {
            get => _infrastructureType;
            set
            {
                if (IsMainDataStorage)
                    return;

                _infrastructureType = value;
            }
        }

        /// <summary>
        /// Наименование провайдера БД
        /// </summary>
        private string _providerName = string.Empty;

        /// <summary>
        /// Наименование провайдера БД
        /// </summary>
        public string ProviderName
        {
            get => _providerName;
            set
            {
                if (IsMainDataStorage)
                    return;

                _providerName = value;
            }
        }

        /// <summary>
        /// Строки подключения к БД для разных групп сущностей
        /// </summary>
        private Dictionary<InfrastructureEntityGroups, string> _connectionStrings = new Dictionary<InfrastructureEntityGroups, string>();

        /// <summary>
        /// Строки подключения к БД для разных групп сущностей
        /// </summary>
        public Dictionary<InfrastructureEntityGroups, string> ConnectionStrings
        {
            get
            {
                return _connectionStrings;
            }
            internal set
            {
                _connectionStrings = value ?? new Dictionary<InfrastructureEntityGroups, string>();
            }
        }

        /// <summary>
        /// Репозитории БД
        /// </summary>
        private Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> _infrastructureRepositories = new Dictionary<InfrastructureEntityGroups, IInfrastructureRepository>();

        /// <summary>
        /// Репозитории БД
        /// </summary>
        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories
        {
            get
            {
                return _infrastructureRepositories;
            }
            internal set
            {
                _infrastructureRepositories = value ?? new Dictionary<InfrastructureEntityGroups, IInfrastructureRepository>();
            }
        }

        /// <summary>
        /// Репозиторий БД работы с репозиториями Чубушника
        /// </summary>
        public IPhiladelphusRepositoriesInfrastructureRepository PhiladelphusRepositoriesInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                var repositories = InfrastructureRepositories;
                if (repositories == null || repositories.ContainsKey(InfrastructureEntityGroups.PhiladelphusRepositories) == false)
                    return null;
                return (IPhiladelphusRepositoriesInfrastructureRepository)repositories[InfrastructureEntityGroups.PhiladelphusRepositories];
            }
        }

        /// <summary>
        /// Репозиторий БД работы с участниками кустарника репозитория Чубушника
        /// </summary>
        public IShrubMembersInfrastructureRepository ShrubMembersInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                var repositories = InfrastructureRepositories;
                if (repositories == null || repositories.ContainsKey(InfrastructureEntityGroups.ShrubMembers) == false)
                    return null;
                return (IShrubMembersInfrastructureRepository)repositories[InfrastructureEntityGroups.ShrubMembers];
            }
        }

        /// <summary>
        /// Репозиторий БД работы с отчетами
        /// </summary>
        public IReportsInfrastructureRepository ReportsInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                var repositories = InfrastructureRepositories;
                if (repositories == null || repositories.ContainsKey(InfrastructureEntityGroups.Reports) == false)
                    return null;
                return (IReportsInfrastructureRepository)repositories[InfrastructureEntityGroups.Reports];
            }
        }

        /// <summary>
        /// Имеет репозиторий БД работы с репозиториями Чубушника 
        /// </summary>
        public bool HasPhiladelphusRepositoriesInfrastructureRepository { get => PhiladelphusRepositoriesInfrastructureRepository != null; }

        /// <summary>
        /// Имеет репозиторий БД работы с участниками кустарника репозитория Чубушника
        /// </summary>
        public bool HasShrubMembersInfrastructureRepository { get => ShrubMembersInfrastructureRepository != null; }

        /// <summary>
        /// Имеет репозиторий БД работы с отчетами
        /// </summary>
        public bool HasReportsInfrastructureRepository { get => ReportsInfrastructureRepository != null; }

        /// <summary>
        /// Основное хранилище данных
        /// </summary>
        public bool IsMainDataStorage { get; } = false;

        /// <summary>
        /// Доступность хранилища (доступность всех репозиториев БД)
        /// </summary>
        private bool _isAvailable = false;

        /// <summary>
        /// Доступность хранилища (доступность всех репозиториев БД)
        /// </summary>
        public bool IsAvailable { get => _isAvailable; }

        /// <summary>
        /// Состояние отключенности
        /// </summary>
        private bool _isDisabled = false;

        /// <summary>
        /// Состояние отключенности
        /// </summary>
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (IsMainDataStorage)
                    return;

                _isDisabled = value;
            }
        }

        /// <summary>
        /// Признак скрытого элемента
        /// </summary>
        private bool _isHidden = false;

        /// <summary>
        /// Признак скрытого элемента
        /// </summary>
        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                if (IsMainDataStorage)
                    return;

                _isHidden = value;
            }
        }


        /// <summary>
        /// Время последней проверки доступности
        /// </summary>
        private DateTime _lastCheckTime;

        /// <summary>
        /// Время последней проверки доступности
        /// </summary>
        public DateTime LastCheckTime { get => _lastCheckTime; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="name">Наименование</param>
        /// <param name="description">Описание</param>
        /// <param name="infrastructureType">Тип</param>
        /// <param name="providerName">Наименование провайдера БД</param>
        /// <param name="connectionStrings">Строки подключения к БД для разных групп сущностей</param>
        /// <param name="isDisabled">Состояние отключенности</param>
        /// <param name="isHidden">Признак скрытого элемента.</param>
        internal DataStorageModel(
            ILogger logger,
            Guid uuid, 
            string name, 
            string description,
            InfrastructureTypes infrastructureType, 
            bool isDisabled,
            bool isHidden,
            string providerName = "",
            Dictionary<InfrastructureEntityGroups, string> connectionStrings = null)
        {
            _logger = logger;
            Uuid = uuid;
            if (uuid == MainDataStorageUuid)
            {
                IsMainDataStorage = true;
            }

            _name = name;
            _description = description;
            _infrastructureType = infrastructureType;
            _providerName = providerName ?? string.Empty;
            ConnectionStrings = connectionStrings ?? new Dictionary<InfrastructureEntityGroups, string>();
            IsDisabled = isDisabled;
            IsHidden = isHidden;
            InfrastructureRepositories = new Dictionary<InfrastructureEntityGroups, IInfrastructureRepository>();
        }

        /// <summary>
        /// Запустить автоматическую проверку доступности всех репозиториев
        /// </summary>
        /// <param name="interval">Интервал проверки (сек.). Не рекомендуется устанавливать период менее 60 сек.</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool StartAvailableAutoChecking(int interval = 60)
        {
            if (_isDisabled)
                return false;
            if (_isAutoChecking)
                return false;

            _timer = new Timer(interval * 1000);
            _timer.Elapsed += (s, e) => _ = Task.Run(async () =>
            {
                try
                {
                    await CheckAvailableAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Ошибка проверки доступности хранилища {Name}");
                }
            });
            _timer.AutoReset = true;
            _timer.Enabled = true;

            _isAutoChecking = true;
            _logger.Information($"Task '{Task.CurrentId}'. Хранилище '{Name}'. Начало автоматической проверки доступности каждые {interval} сек.");

            return true;
        }

        /// <summary>
        /// Проверить доступность всех репозиториев
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public bool CheckAvailable()
        {
            return CheckAvailableAsync().Result;
        }

        /// <summary>
        /// Проверить доступность всех репозиториев
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public Task<bool> CheckAvailableAsync()
        {
            var task = Task.Run(() =>
            {
                if (_isDisabled)
                {
                    _isAvailable = false;
                }
                else
                {
                    _logger.Information($"Task '{Task.CurrentId}'. Хранилище '{Name}'. Проверка доступности от {DateTime.Now}");

                    var result = InfrastructureRepositories.Count != 0;
                    foreach (var item in InfrastructureRepositories)
                    {
                        if (item.Value.CheckAvailability() == false)
                        {
                            result = false;
                            break;
                        }
                    }
                    _isAvailable = result;

                    _logger.Information($"Task '{Task.CurrentId}'. Хранилище '{Name}' - доступность {_isAvailable}.");
                }

                _lastCheckTime = DateTime.Now;

                return _isAvailable;
            });
            return task;
        }

        /// <summary>
        /// Остановить автоматическую проверку доступности
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public bool StopAvailableAutoChecking()
        {
            if (_isAutoChecking == false)
                return false;

            _timer?.Stop();

            _isAutoChecking = false;
            _logger.Information($"Task '{Task.CurrentId}'. Хранилище '{Name}'. Остановка автоматической проверки доступности.");

            return true;
        }

        /// <summary>
        /// Выполняет операцию Dispose.
        /// </summary>
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
    }
}
