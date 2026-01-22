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

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public TreeRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private TreeRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        public TreeRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }

        public ApplicationVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<ApplicationVM> logger,
            INotificationService notificationService,
            IOptions<ApplicationSettings> options,
            ApplicationCommandsVM applicationCommandsVM,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            TreeRepositoryCollectionVM treeRepositoryCollectionVM,
            TreeRepositoryHeadersCollectionVM treeRepositoryHeadersCollectionVM,
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
            _repositoryCollectionVM = treeRepositoryCollectionVM;
            _repositoryHeadersCollectionVM = treeRepositoryHeadersCollectionVM;
            //_repositoryCreationVM = RepositoryCreationVM;
            _launchVM = launchVM;

            //CultureInfo.CurrentCulture = new CultureInfo("ru-RU");


            //var treeRepositoryCollectionService = new TreeRepositoryCollectionService(configDirectory);

            //_applicationWindowsVM = new ApplicationWindowsVM();
            //_applicationCommandsVM = new ApplicationCommandsVM(this, _applicationWindowsVM);
            //_dataStoragesSettingsVM = new DataStoragesSettingsVM();
            //_repositoryCollectionVM = new TreeRepositoryCollectionVM(treeRepositoryCollectionService, _dataStoragesSettingsVM);
            //_repositoryHeadersCollectionVM = new TreeRepositoryHeadersCollectionVM(treeRepositoryCollectionService);
            //_repositoryCreationVM = new RepositoryCreationVM(treeRepositoryCollectionService, _repositoryCollectionVM, _dataStoragesSettingsVM);
            //_launchVM = new LaunchVM(_dataStoragesSettingsVM, _repositoryCollectionVM, _repositoryHeadersCollectionVM, _applicationCommandsVM.OpenMainWindowCommand);
            //_applicationWindowsVM.LaunchWindow = new LaunchWindow(_launchVM);
            //_applicationWindowsVM.LaunchWindow.Show();
            //_applicationWindowsVM.LaunchWindow.Focus();
            //_applicationWindowsVM.LaunchWindow.Activate();
        }

    }
}
