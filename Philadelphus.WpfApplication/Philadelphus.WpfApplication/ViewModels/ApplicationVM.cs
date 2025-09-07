using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Handlers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.WpfApplication.ViewModels.SupportiveViewModels;
using Philadelphus.WpfApplication.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {
        private static DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM
        {
            get
            {
                if (_dataStoragesSettingsVM == null)
                {
                    _dataStoragesSettingsVM = new DataStoragesSettingsVM();
                }
                return _dataStoragesSettingsVM;
            }
        }
        private NotificationsVM _notificationsVM;
        public NotificationsVM NotificationsVM
        { 
            get 
            {
                if (_notificationsVM == null)
                {
                    _notificationsVM = new NotificationsVM();
                }
                return _notificationsVM; 
            } 
        }

        private StartWindow _startWindow;
        private MainWindow _mainWindow;

        public ApplicationViewModel()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            _startWindow = new StartWindow() { DataContext = this };
            _startWindow.Show();
            // ВРЕМЕННО!!!
            //RepositoryCollectionViewModel.CurrentRepositoryExplorerVM = RepositoryCollectionViewModel.TreeRepositoriesVMs.FirstOrDefault();
            // ВРЕМЕННО!!!
            //_mainWindow = new MainWindow() { DataContext = this };
        }

        public string UserName = "Sergey";
        public string Title 
        { 
            get
            {
                var title = "Чубушник";
                var repositoryName = _repositoryCollectionViewModel?.CurrentRepositoryExplorerVM?.TreeRepository?.Name;
                if (String.IsNullOrEmpty(repositoryName) == false)
                {
                    title = $"{repositoryName} - Чубушник";
                }
                return title;
            }
        }

        private RepositoryCollectionVM _repositoryCollectionViewModel = new RepositoryCollectionVM() { DataStoragesSettingsVM = _dataStoragesSettingsVM };        
        public RepositoryCollectionVM RepositoryCollectionViewModel
        {
            get 
            { 
                return _repositoryCollectionViewModel; 
            } 
        }

        private RepositoryExplorerVM _repositoryViewModel = new RepositoryExplorerVM();
        public RepositoryExplorerVM RepositoryExplorerViewModel 
        { 
            get 
            { 
                return _repositoryViewModel; 
            } 
        }

        private int _currentProgress = 0;
        public int CurrentProgress 
        {  
            get 
            {
                return _currentProgress; 
            } 
            set 
            {
                _currentProgress = value;
                OnPropertyChanged(nameof(CurrentProgress));
            } 
        }

        

        #region  [Commands]
        public RelayCommand OpenMainWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_mainWindow == null)
                        _mainWindow = new MainWindow() { DataContext = this };
                    _mainWindow.Show();
                    
                    //_startWindow.Visibility = Visibility.Hidden;
                    _startWindow.Close();
                    _notificationsVM = new NotificationsVM();
                    OnPropertyChanged(nameof(PopupVM));
                });
            }
        }
        public RelayCommand OpenDataStorageSettings
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var window = new DataStoragesSettingsWindow() { DataContext = DataStoragesSettingsVM };
                    window.Show();
                });
            }
        }
        //public RelayCommand AddNewTreeRepositoryCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(obj =>
        //        {
        //            var repository = new TreeRepository(Guid.Empty);
        //            _treeRepositoriesVMs.Add(repository);
        //            OnPropertyChanged(nameof(TreeRepositoriesVMs));
        //            _currentTreeRepository = TreeRepositoriesVMs.Last();
        //            OnPropertyChanged(nameof(CurrentTreeRepository));
        //        });
        //    }
        //}
        //public RelayCommand SaveRepositoryCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(obj =>
        //        {
        //            var service = new DataTreeProsessingService();
        //            service.ModifyRepository(_currentTreeRepository);
        //        });
        //    }
        //}
        //public RelayCommand RefreshRepositoryListCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(obj =>
        //        {
        //            var service = new DataTreeProsessingService();
        //            _treeRepositoriesVMs = service.GetRepositories() ?? new List<TreeRepository>();
        //            OnPropertyChanged(nameof(TreeRepositoriesVMs));
        //        });
        //    }
        //}
        #endregion
    }
}
