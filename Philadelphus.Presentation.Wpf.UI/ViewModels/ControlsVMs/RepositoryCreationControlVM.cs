using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;
using System.IO;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для репозитория.
    /// </summary>
    public class RepositoryCreationControlVM : ControlBaseVM
    {
        private readonly IPhiladelphusRepositoryCollectionService _collectionService;
        private readonly IPhiladelphusRepositoryService _repositoryService;
        private readonly IFileDialogService? _fileDialogService;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<PhiladelphusRepositoryHeadersCollectionConfig> _headersCollectionConfig;
        private readonly FileInfo _configFile;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
        private readonly PhiladelphusRepositoryCollectionVM _repositoryCollectionVM;
        private readonly PhiladelphusRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        private readonly IRelayCommandFactory _commandFactory;

        private string _name;
        private string _description;
        
        public string Name 
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        /// <summary>
        /// Коллекция хранилищ данных.
        /// </summary>
        public DataStoragesCollectionVM DataStoragesCollectionVM { get => _dataStoragesCollectionVM; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryCreationControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="collectionService">Параметр collectionService.</param>
        /// <param name="repositoryService">Параметр repositoryService.</param>
        /// <param name="configurationService">Параметр configurationService.</param>
        /// <param name="repositoryCollectionVM">Параметр repositoryCollectionVM.</param>
        /// <param name="repositoryHeadersCollectionVM">Параметр repositoryHeadersCollectionVM.</param>
        /// <param name="dataStoragesSettingsVM">Параметр dataStoragesSettingsVM.</param>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <param name="headersCollectionConfig">Параметр headersCollectionConfig.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public RepositoryCreationControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryCollectionService collectionService,
            IPhiladelphusRepositoryService repositoryService,
            IConfigurationService configurationService,
            PhiladelphusRepositoryCollectionVM repositoryCollectionVM,
            PhiladelphusRepositoryHeadersCollectionVM repositoryHeadersCollectionVM,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            IOptions<ApplicationSettingsConfig> options,
            IOptions<PhiladelphusRepositoryHeadersCollectionConfig> headersCollectionConfig,
            IApplicationCommandsVM applicationCommandsVM,
            IFileDialogService fileDialogService,
            IRelayCommandFactory commandFactory)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(collectionService);
            ArgumentNullException.ThrowIfNull(repositoryService);
            ArgumentNullException.ThrowIfNull(fileDialogService);
            ArgumentNullException.ThrowIfNull(configurationService);
            ArgumentNullException.ThrowIfNull(repositoryCollectionVM);
            ArgumentNullException.ThrowIfNull(repositoryHeadersCollectionVM);
            ArgumentNullException.ThrowIfNull(dataStoragesSettingsVM);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(headersCollectionConfig);
            ArgumentNullException.ThrowIfNull(headersCollectionConfig.Value);
            ArgumentNullException.ThrowIfNull(commandFactory);

            _collectionService = collectionService;
            _repositoryService = repositoryService;
            _fileDialogService = fileDialogService;
            _configurationService = configurationService;
            _headersCollectionConfig = headersCollectionConfig;
            _commandFactory = commandFactory;

            _dataStoragesCollectionVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;

            _configFile = options.Value.RepositoryHeadersConfigFullPath;

            if (_dataStoragesCollectionVM.DataStoragesVMs?.Contains(_dataStoragesCollectionVM?.MainDataStorageVM) ?? false)
            {
                _dataStoragesCollectionVM.SelectedDataStorageVM = _dataStoragesCollectionVM.MainDataStorageVM;
            }
            Name = "Новый репозиторий";
        }
        public IRelayCommand CreateAndSaveRepositoryCommand
        {
            get
            {
                return _commandFactory.Create(obj =>
                {
                    if (string.IsNullOrEmpty(Name)
                        || _dataStoragesCollectionVM.SelectedDataStorageVM == null)
                    {
                        _notificationService.SendModalWindow<RepositoryCreationControlVM>(
                            "Некорректно заполнены параметры, операция не выполнена.", 
                            NotificationCriticalLevelModel.Warning);
                        return;
                    }

                    var existsRepository = _repositoryCollectionVM.PhiladelphusRepositoriesVMs.FirstOrDefault(x => x.Name == _name);
                    if (existsRepository != null)
                    {
                        _notificationService.SendModalWindow<RepositoryCreationControlVM>(
                            $"Репозиторий '{existsRepository?.Name}' [{existsRepository?.Uuid}] уже существует в хранилище '{existsRepository?.OwnDataStorageName}' [{existsRepository?.OwnDataStorageUuid}], операция не выполнена. Откройте репозиторий из списка доступных.",
                            NotificationCriticalLevelModel.Warning);
                        return;
                    }

                    var model = _collectionService.CreateNewPhiladelphusRepository(_dataStoragesCollectionVM.SelectedDataStorageVM.Model);
                    model.Name = Name;
                    model.Description = Description;
                    _collectionService.SaveChanges(ref model);

                    var vm = new PhiladelphusRepositoryVM(model, _dataStoragesCollectionVM, _repositoryService, _fileDialogService);
                    
                    _repositoryCollectionVM.PhiladelphusRepositoriesVMs.Add(vm);
                    _repositoryCollectionVM.CurrentRepositoryVM = vm;

                    var headerVm = _repositoryHeadersCollectionVM.AddPhiladelphusRepositoryHeaderVMFromPhiladelphusRepositoryVM(vm);
                });
            }
        }

        /// <summary>
        /// Команда выполнения операции хранилища данных.
        /// </summary>
        public IRelayCommand OpenDataStoragesSettingsControlCommand { get; set; }
    }
}
