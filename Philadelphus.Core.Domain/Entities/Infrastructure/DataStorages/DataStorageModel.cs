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
        private readonly ILogger _logger;
        private System.Timers.Timer? _timer;

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Тип хранилища данных
        /// </summary>
        public InfrastructureTypes InfrastructureType { get; set; }

        /// <summary>
        /// Репозитории БД
        /// </summary>
        private Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> _infrastructureRepositories;

        /// <summary>
        /// Репозитории БД
        /// </summary>
        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories 
        { 
            get
            {
                if (_isDisabled)
                    return null;
                return _infrastructureRepositories;
            }
            internal set
            {
                if (_isDisabled)
                    return;
                _infrastructureRepositories = value;
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
                if (InfrastructureRepositories.ContainsKey(InfrastructureEntityGroups.PhiladelphusRepositories) == false)
                    return null;
                return (IPhiladelphusRepositoriesInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.PhiladelphusRepositories];
            }
        }

        /// <summary>
        /// Репозиторий БД работы с участниками репозитория Чубушника
        /// </summary>
        public IPhiladelphusRepositoriesMembersInfrastructureRepository PhiladelphusRepositoryMembersInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                if (InfrastructureRepositories.ContainsKey(InfrastructureEntityGroups.MainEntities) == false)
                    return null;
                return (IPhiladelphusRepositoriesMembersInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.MainEntities];
            }
        }

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
        private bool _isDisabled;

        /// <summary>
        /// Состояние отключенности
        /// </summary>
        public bool IsDisabled { get => _isDisabled; set => _isDisabled = value; }


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
        /// <param name="isDisabled">Состояние отключенности</param>
        internal DataStorageModel(
            ILogger logger,
            Guid uuid, 
            string name, 
            string description,
            InfrastructureTypes infrastructureType, 
            bool isDisabled)
        {
            _logger = logger;
            Uuid = uuid;
            Name = name;
            Description = description;
            InfrastructureType = infrastructureType;
            IsDisabled = isDisabled;
            if (IsDisabled == false)
            {
                InfrastructureRepositories = new Dictionary<InfrastructureEntityGroups, IInfrastructureRepository>();
            }
            Task.Run(() => CheckAvailable());
        }

        /// <summary>
        /// Запустить автоматическую проверку доступности всех репозиториев
        /// </summary>
        /// <param name="interval">Интервал проверки (сек.). Не рекомендуется устанавливать период менее 60 сек.</param>
        /// <returns></returns>
        public bool StartAvailableAutoChecking(int interval = 60)
        {
             if (_isDisabled)
                return false;
            _timer = new Timer(interval * 1000);
            _timer.Elapsed += OnAvailabilityCheckTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _logger.Information($"Task '{Task.CurrentId}'. Хранилище '{Name}'. Начало автоматической проверки доступности каждые {interval} сек.");
            return true;
        }

        private void OnAvailabilityCheckTimerElapsed(object source, ElapsedEventArgs e)
        {
            _ = Task.Run(() => CheckAvailable());
        }

        /// <summary>
        /// Проверить доступность всех репозиториев
        /// </summary>
        /// <returns></returns>
        public bool CheckAvailable()
        {
            if (_isDisabled)
            {
                _isAvailable = false;
            }
            else
            {
                _logger.Information($"Task '{Task.CurrentId}'. Хранилище '{Name}'. Проверка доступности от {DateTime.Now}");

                var result = true; 
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
        }

        /// <summary>
        /// Проверить доступность всех репозиториев (обертка для таймера)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void CheckAvailable(Object source, ElapsedEventArgs e)
        {
            CheckAvailable();
        }

        /// <summary>
        /// Остановить автоматическую проверку доступности
        /// </summary>
        /// <returns></returns>
        public bool StopAvailableAutoChecking()
        {
            _timer?.Stop();
            return true;
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
    }
}
