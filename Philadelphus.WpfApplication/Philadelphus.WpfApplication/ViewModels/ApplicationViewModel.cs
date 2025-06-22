using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.WpfApplication.Views;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {
        private RepositoryCollectionViewModel _repositoryCollectionViewModel = new RepositoryCollectionViewModel();
        public RepositoryCollectionViewModel RepositoryCollectionViewModel
        {
            get 
            { 
                return _repositoryCollectionViewModel; 
            } 
        }

        private RepositoryExplorerViewModel _repositoryViewModel = new RepositoryExplorerViewModel();
        public RepositoryExplorerViewModel RepositoryExplorerViewModel 
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
        #region  [Commands]OpenRepositoryCollectionSettingsWindow
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
        //            var service = new DataTreeRepositoryService();
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
        //            var service = new DataTreeRepositoryService();
        //            _treeRepositoriesVMs = service.GetRepositories() ?? new List<TreeRepository>();
        //            OnPropertyChanged(nameof(TreeRepositoriesVMs));
        //        });
        //    }
        //}
        #endregion
    }
}
