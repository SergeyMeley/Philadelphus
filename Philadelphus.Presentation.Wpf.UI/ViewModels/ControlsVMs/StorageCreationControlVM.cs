using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Presentation.Wpf.UI.Factories.Implementations;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Serilog;
using System.IO;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для элемента управления.
    /// </summary>
    public class StorageCreationControlVM : ControlBaseVM
    {
        private readonly IDataStoragesService _service;
        private readonly IConfigurationService _configurationService;
        private readonly IInfrastructureRepositoryFactory _infrastructureRepositoryFactory;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollectionConfig;
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
        /// Контейнер строк подключения.
        /// </summary>
        public List<ConnectionStringsContainer> ConnectionStringsContainers { get => _connectionStringsCollectionConfig.Value.ConnectionStringsContainers; }
      
        /// <summary>
        /// Контейнер строк подключения.
        /// </summary>
        public ConnectionStringsContainer SelectedConnectionStringsContainer { get; set; }

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
        /// <param name="connectionStringsCollectionConfig">Параметр connectionStringsCollectionConfig.</param>
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
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollectionConfig,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollectionConfig,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(configurationService);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM);
            ArgumentNullException.ThrowIfNull(infrastructureRepositoryFactory);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(connectionStringsCollectionConfig);
            ArgumentNullException.ThrowIfNull(connectionStringsCollectionConfig.Value);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionConfig);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionConfig.Value);

            _service = service;
            _configurationService = configurationService;
            _connectionStringsCollectionConfig = connectionStringsCollectionConfig;
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
                    if (string.IsNullOrEmpty(Name)
                        || SelectedConnectionStringsContainer == null)
                    {
                        MessageBox.Show($"Некорректно заполнены параметры, операция не выполнена.");
                        return;
                    }
                    if (_dataStoragesCollectionConfig.Value.DataStorages.Any(x => x.Name == Name))
                    {
                        MessageBox.Show($"Хранилище '{Name}' уже существует, операция не выполнена.");
                        return;
                    }

                    var model = _service.CreateDataStorageModel(Name, Description, SelectedConnectionStringsContainer,
                        (csc, type, group) =>
                        {
                            csc.ConnectionStrings.TryGetValue(group, out var cs);
                            return _infrastructureRepositoryFactory.Create(type, group, cs);
                        });
                    var vm = new DataStorageVM(model);
                    var entity = _mapper.Map<DataStorage>(model);
                    _dataStoragesCollectionConfig.Value.DataStorages.Add(entity);
                    _configurationService.UpdateConfigFile<DataStoragesCollectionConfig>(_configFile, _dataStoragesCollectionConfig);
                    _dataStoragesCollectionVM.DataStoragesVMs.Add(vm);
                    _dataStoragesCollectionVM.SelectedDataStorageVM = vm;
                });
            }
        }

        /// <summary>
        /// Команда выполнения операции элемента управления.
        /// </summary>
        public RelayCommand OpenConnectionStringsSettingsControlCommand { get; set; }
    }
}
