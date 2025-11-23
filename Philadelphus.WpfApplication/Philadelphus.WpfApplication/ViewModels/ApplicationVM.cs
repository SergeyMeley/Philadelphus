using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Business.Config;
using Philadelphus.Business.Services;
using Philadelphus.Business.Services.Interfaces;
using Philadelphus.WpfApplication.ViewModels.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using Philadelphus.WpfApplication.ViewModels.SupportiveVMs;
using Philadelphus.WpfApplication.Views.Windows;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationVM : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplicationVM> _logger;
        private readonly INotificationService _notificationService;

        private ApplicationCommandsVM _applicationCommandsVM;
        public ApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }

        private LaunchVM _launchVM;
        public LaunchVM LaunchVM { get { return _launchVM; } }

        private RepositoryCreationVM _repositoryCreationVM;
        public RepositoryCreationVM RepositoryCreationVM { get => _repositoryCreationVM; }

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public TreeRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private TreeRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        public TreeRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }

        private NotificationsVM _notificationsVM;
        public NotificationsVM NotificationsVM { get => _notificationsVM; }
        public ViewModelBase SelectedElementVM { get => RepositoryCollectionVM.CurrentRepositoryExplorerVM; } //TODO: Временно только элементы репозитория
        public ApplicationVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<ApplicationVM> logger,
            INotificationService notificationService,
            IOptions<ApplicationSettings> options,
            ApplicationCommandsVM applicationCommandsVM,
            DataStoragesSettingsVM dataStoragesSettingsVM,
            TreeRepositoryCollectionVM treeRepositoryCollectionVM,
            TreeRepositoryHeadersCollectionVM treeRepositoryHeadersCollectionVM,
            RepositoryCreationVM RepositoryCreationVM,
            LaunchVM launchVM)
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
            _repositoryCreationVM = RepositoryCreationVM;
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

        public string Title 
        { 
            get
            {
                var title = "Чубушник";
                var repositoryName = _repositoryCollectionVM?.CurrentRepositoryExplorerVM?.Name;
                if (String.IsNullOrEmpty(repositoryName) == false)
                {
                    title = $"{repositoryName} - Чубушник";
                }
                return title;
            }
        }

        public TreeRepositoryVM RepositoryExplorerVM 
        { 
            get 
            { 
                return _repositoryCollectionVM.CurrentRepositoryExplorerVM; 
            } 
            set
            {
                _repositoryCollectionVM.CurrentRepositoryExplorerVM = value;
            }
        }
    }
}
