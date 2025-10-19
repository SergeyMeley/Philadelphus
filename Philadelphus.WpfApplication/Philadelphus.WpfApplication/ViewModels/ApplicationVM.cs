using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Handlers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.WpfApplication.Models.StorageConfig;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using Philadelphus.WpfApplication.ViewModels.SupportiveViewModels;
using Philadelphus.WpfApplication.Views;
using Philadelphus.WpfApplication.Views.Controls;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Windows;

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

        private RepositoryCollectionVM _repositoryCollectionVM;
        public RepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private NotificationsVM _notificationsVM;
        public NotificationsVM NotificationsVM { get => _notificationsVM; }
        public ApplicationVM()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            _applicationWindowsVM = new ApplicationWindowsVM();
            _applicationCommandsVM = new ApplicationCommandsVM(this, _applicationWindowsVM);
            _dataStoragesSettingsVM = new DataStoragesSettingsVM();
            _repositoryCollectionVM = new RepositoryCollectionVM(_dataStoragesSettingsVM);
            _repositoryCreationVM = new RepositoryCreationVM(_repositoryCollectionVM, _applicationCommandsVM.OpenDataStoragesSettingsWindowCommand, _dataStoragesSettingsVM);
            _launchVM = new LaunchVM(_dataStoragesSettingsVM, _repositoryCollectionVM, _applicationCommandsVM.OpenRepositoryCreationWindowCommand, _applicationCommandsVM.OpenMainWindowCommand);
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
