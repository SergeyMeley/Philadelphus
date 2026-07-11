using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Serilog;
using System.IO;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для элемента управления.
    /// </summary>
    public class StorageCreationControlVM : ControlBaseVM
    {
        private readonly IDataStoragesService _service;
        private readonly IConfigurationService _configurationService;
        private readonly IInfrastructureRepositoryFactory _infrastructureRepositoryFactory;
        private readonly IOptions<DataStoragesCollectionConfig> _dataStoragesCollectionConfig;
        private readonly FileInfo _configFile;

        private string _name;

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get => _name; set => _name = value; }

        private string _description;

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get => _description; set => _description = value; }
       
        /// <summary>
        /// Тип инфраструктуры создаваемого хранилища.
        /// </summary>
        private InfrastructureTypes _infrastructureType = InfrastructureTypes.SQLiteEf;

        public InfrastructureTypes InfrastructureType
        {
            get
            {
                return _infrastructureType;
            }
            set
            {
                if (_infrastructureType == value)
                    return;

                _infrastructureType = value;
                OnPropertyChanged(nameof(InfrastructureType));
                OnPropertyChanged(nameof(IsSqliteInfrastructure));
            }
        }

        public bool IsSqliteInfrastructure
        {
            get
            {
                return InfrastructureType == InfrastructureTypes.SQLiteEf;
            }
        }
      
        /// <summary>
        /// Доступные типы инфраструктуры.
        /// </summary>
        public IEnumerable<InfrastructureTypes> AvailableInfrastructureTypes { get; }
            = Enum.GetValues<InfrastructureTypes>();

        /// <summary>
        /// Наименование провайдера создаваемого хранилища.
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Строки подключения создаваемого хранилища по группам сущностей.
        /// </summary>
        public IEnumerable<DataStorageConnectionStringVM> ConnectionStrings { get; }
            = Enum.GetValues<InfrastructureEntityGroups>()
                .Select(group => new DataStorageConnectionStringVM(group, string.Empty, createSqliteEditor: true))
                .ToList();

        private DataStoragesCollectionVM _dataStoragesCollectionVM;
      
        /// <summary>
        /// Коллекция хранилищ данных.
        /// </summary>
        public DataStoragesCollectionVM DataStoragesCollectionVM { get => _dataStoragesCollectionVM; set => _dataStoragesCollectionVM = value; }
     
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StorageCreationControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <param name="configurationService">Параметр configurationService.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="infrastructureRepositoryFactory">Параметр infrastructureRepositoryFactory.</param>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <param name="dataStoragesCollectionConfig">Параметр dataStoragesCollectionConfig.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public StorageCreationControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IDataStoragesService service,
            IConfigurationService configurationService,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IInfrastructureRepositoryFactory infrastructureRepositoryFactory,
            IOptions<ApplicationSettingsConfig> options,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollectionConfig,
            IApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(configurationService);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM);
            ArgumentNullException.ThrowIfNull(infrastructureRepositoryFactory);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionConfig);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionConfig.Value);

            _service = service;
            _configurationService = configurationService;
            _dataStoragesCollectionConfig = dataStoragesCollectionConfig;

            _dataStoragesCollectionVM = dataStoragesCollectionVM;
            _infrastructureRepositoryFactory = infrastructureRepositoryFactory;

            _configFile = options.Value.StoragesConfigFullPath;
        }
        public RelayCommand CreateAndSaveDataStorageCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (string.IsNullOrWhiteSpace(Name)
                        || string.IsNullOrWhiteSpace(ProviderName)
                        || ConnectionStrings.All(x => string.IsNullOrWhiteSpace(x.ConnectionString))
                        || IsSqliteInfrastructure &&
                            (ConnectionStrings.All(x => x.SqliteEditor!.IsValid == false)
                            || ConnectionStrings.Any(x => x.SqliteEditor!.IsEmpty == false && x.SqliteEditor.IsValid == false)))
                    {
                        _notificationService.SendModalWindow<StorageCreationControlVM>(
                            "Некорректно заполнены параметры, операция не выполнена.", 
                            NotificationCriticalLevelModel.Warning);
                        return;
                    }
                    if (_dataStoragesCollectionConfig.Value.DataStorages.Any(x => x.Name == Name))
                    {
                        _notificationService.SendModalWindow<StorageCreationControlVM>(
                            $"Хранилище '{Name}' уже существует, операция не выполнена.", 
                            NotificationCriticalLevelModel.Warning);
                        return;
                    }

                    var connectionStringsContainer = new ConnectionStringsContainer
                    {
                        StorageUuid = Guid.CreateVersion7(),
                        InfrastructureType = InfrastructureType,
                        ProviderName = ProviderName,
                        ConnectionStrings = ConnectionStrings.ToDictionary(x => x.EntityGroup, x => x.ConnectionString)
                    };

                    var model = _service.CreateDataStorageModel(Name, Description, connectionStringsContainer,
                        (csc, type, group) =>
                        {
                            csc.ConnectionStrings.TryGetValue(group, out var cs);
                            return _infrastructureRepositoryFactory.Create(type, group, cs);
                        });
                    var vm = new DataStorageVM(model, _notificationService);
                    var entity = _mapper.Map<DataStorage>(model);
                    _dataStoragesCollectionConfig.Value.DataStorages.Add(entity);
                    _configurationService.UpdateConfigFile<DataStoragesCollectionConfig>(_configFile, _dataStoragesCollectionConfig);
                    _dataStoragesCollectionVM.DataStoragesVMs.Add(vm);
                    _dataStoragesCollectionVM.SelectedDataStorageVM = vm;
                });
            }
        }

    }
}
