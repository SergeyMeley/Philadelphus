using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    /// <summary>
    /// Модель представления для приложения.
    /// </summary>
    public class ApplicationVM : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;

        private ApplicationCommandsVM _applicationCommandsVM;
     
        /// <summary>
        /// Команды приложения.
        /// </summary>
        public ApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }

        private LaunchWindowVM _launchVM;
       
        /// <summary>
        /// Модель представления стартового окна.
        /// </summary>
        public LaunchWindowVM LaunchVM { get { return _launchVM; } }

        private DataStoragesCollectionVM _dataStoragesSettingsVM;
       
        /// <summary>
        /// Хранилище данных.
        /// </summary>
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }

        private PhiladelphusRepositoryCollectionVM _repositoryCollectionVM;
      
        /// <summary>
        /// Репозиторий.
        /// </summary>
        public PhiladelphusRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private PhiladelphusRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
       
        /// <summary>
        /// Репозиторий.
        /// </summary>
        public PhiladelphusRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="dataStoragesSettingsVM">Параметр dataStoragesSettingsVM.</param>
        /// <param name="PhiladelphusRepositoryCollectionVM">Параметр PhiladelphusRepositoryCollectionVM.</param>
        /// <param name="PhiladelphusRepositoryHeadersCollectionVM">Параметр PhiladelphusRepositoryHeadersCollectionVM.</param>
        /// <param name="RepositoryCreationVM">Параметр RepositoryCreationVM.</param>
        /// <param name="launchVM">Параметр launchVM.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ApplicationVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IOptions<ApplicationSettingsConfig> options,
            ApplicationCommandsVM applicationCommandsVM,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            PhiladelphusRepositoryCollectionVM PhiladelphusRepositoryCollectionVM,
            PhiladelphusRepositoryHeadersCollectionVM PhiladelphusRepositoryHeadersCollectionVM,
            RepositoryCreationControlVM RepositoryCreationVM,
            LaunchWindowVM launchVM)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(applicationCommandsVM);
            ArgumentNullException.ThrowIfNull(dataStoragesSettingsVM);
            ArgumentNullException.ThrowIfNull(PhiladelphusRepositoryCollectionVM);
            ArgumentNullException.ThrowIfNull(PhiladelphusRepositoryHeadersCollectionVM);
            ArgumentNullException.ThrowIfNull(RepositoryCreationVM);
            ArgumentNullException.ThrowIfNull(launchVM);

            //var configPath = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings.Get("ConfigsDirectory"));
            //if (configPath == null)
            //    throw new Exception("Не найден путь к настроечным файлам. Проверьте параметр \'ConfigsDirectory\' в \'App.config\'.");
            //var configDirectory = new DirectoryInfo(configPath);
            //options.Value.ConfigsDirectory = configDirectory;

            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _applicationCommandsVM = applicationCommandsVM;    // Зависает приложение
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = PhiladelphusRepositoryCollectionVM;
            _repositoryHeadersCollectionVM = PhiladelphusRepositoryHeadersCollectionVM;
            //_repositoryCreationVM = RepositoryCreationVM;
            _launchVM = launchVM;

            //CultureInfo.CurrentCulture = new CultureInfo("ru-RU");


            //var PhiladelphusRepositoryCollectionService = new PhiladelphusRepositoryCollectionService(configDirectory);

            //_applicationWindowsVM = new ApplicationWindowsVM();
            //_applicationCommandsVM = new ApplicationCommandsVM(this, _applicationWindowsVM);
            //_dataStoragesSettingsVM = new DataStoragesSettingsVM();
            //_repositoryCollectionVM = new PhiladelphusRepositoryCollectionVM(PhiladelphusRepositoryCollectionService, _dataStoragesSettingsVM);
            //_repositoryHeadersCollectionVM = new PhiladelphusRepositoryHeadersCollectionVM(PhiladelphusRepositoryCollectionService);
            //_repositoryCreationVM = new RepositoryCreationVM(PhiladelphusRepositoryCollectionService, _repositoryCollectionVM, _dataStoragesSettingsVM);
            //_launchVM = new LaunchVM(_dataStoragesSettingsVM, _repositoryCollectionVM, _repositoryHeadersCollectionVM, _applicationCommandsVM.OpenMainWindowCommand);
            //_applicationWindowsVM.LaunchWindow = new LaunchWindow(_launchVM);
            //_applicationWindowsVM.LaunchWindow.Show();
            //_applicationWindowsVM.LaunchWindow.Focus();
            //_applicationWindowsVM.LaunchWindow.Activate();
        }

    }
}
