using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    public class ApplicationVM : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplicationVM> _logger;
        private readonly INotificationService _notificationService;

        private ApplicationCommandsVM _applicationCommandsVM;
        public ApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }

        private LaunchWindowVM _launchVM;
        public LaunchWindowVM LaunchVM { get { return _launchVM; } }

        private DataStoragesCollectionVM _dataStoragesSettingsVM;
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }

        private PhiladelphusRepositoryCollectionVM _repositoryCollectionVM;
        public PhiladelphusRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private PhiladelphusRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        public PhiladelphusRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }

        public ApplicationVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<ApplicationVM> logger,
            INotificationService notificationService,
            IOptions<ApplicationSettingsConfig> options,
            ApplicationCommandsVM applicationCommandsVM,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            PhiladelphusRepositoryCollectionVM PhiladelphusRepositoryCollectionVM,
            PhiladelphusRepositoryHeadersCollectionVM PhiladelphusRepositoryHeadersCollectionVM,
            RepositoryCreationControlVM RepositoryCreationVM,
            LaunchWindowVM launchVM)
        {
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
