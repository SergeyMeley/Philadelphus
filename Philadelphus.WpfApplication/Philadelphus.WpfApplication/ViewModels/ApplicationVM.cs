using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Handlers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.WpfApplication.Models.StorageConfig;
using Philadelphus.WpfApplication.ViewModels.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using Philadelphus.WpfApplication.ViewModels.SupportiveVMs;
using Philadelphus.WpfApplication.Views.Controls;
using Philadelphus.WpfApplication.Views.Windows;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Windows;
using static System.Environment;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationVM : ViewModelBase
    {
        private ApplicationWindowsVM _applicationWindowsVM;
        public ApplicationWindowsVM ApplicationWindowsVM { get => _applicationWindowsVM; }

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
        public ApplicationVM()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            var configPath = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings.Get("ConfigsDirectory"));
            if (configPath == null)
                throw new Exception("Не найден путь к настроечным файлам. Проверьте параметр \'ConfigsDirectory\' в \'App.config\'.");
            var configDirectory = new DirectoryInfo(configPath);

            var treeRepositoryCollectionService = new TreeRepositoryCollectionService(configDirectory);

            _applicationWindowsVM = new ApplicationWindowsVM();
            _applicationCommandsVM = new ApplicationCommandsVM(this, _applicationWindowsVM);
            _dataStoragesSettingsVM = new DataStoragesSettingsVM();
            _repositoryCollectionVM = new TreeRepositoryCollectionVM(treeRepositoryCollectionService, _dataStoragesSettingsVM);
            _repositoryHeadersCollectionVM = new TreeRepositoryHeadersCollectionVM(treeRepositoryCollectionService);
            _repositoryCreationVM = new RepositoryCreationVM(treeRepositoryCollectionService, _repositoryCollectionVM, _applicationCommandsVM.OpenDataStoragesSettingsWindowCommand, _dataStoragesSettingsVM);
            _launchVM = new LaunchVM(_dataStoragesSettingsVM, _repositoryCollectionVM, _repositoryHeadersCollectionVM, _applicationCommandsVM.OpenRepositoryCreationWindowCommand, _applicationCommandsVM.OpenMainWindowCommand);
            _applicationWindowsVM.LaunchWindow = new LaunchWindow(_launchVM);
            _applicationWindowsVM.LaunchWindow.Show();
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
