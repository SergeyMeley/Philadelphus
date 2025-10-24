using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using Philadelphus.WpfApplication.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class LaunchVM : ViewModelBase
    {
        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }

        private RepositoryCollectionVM _repositoryCollectionVM;
        public RepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }
        public string UserName { get => Environment.UserName; }
        public LaunchVM(DataStoragesSettingsVM dataStoragesSettingsVM, RepositoryCollectionVM repositoryCollectionVM, RelayCommand openRepositoryCreationWindowCommand, RelayCommand openMainWindowCommand)
        {
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            OpenRepositoryCreationWindowCommand = openRepositoryCreationWindowCommand;
            OpenMainWindowCommand = openMainWindowCommand;
        }
        public RelayCommand OpenRepositoryCreationWindowCommand { get; }
        public RelayCommand OpenMainWindowCommand { get; }
    }
}
