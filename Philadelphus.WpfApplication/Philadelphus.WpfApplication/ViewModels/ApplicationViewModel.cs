using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {

        private RepositoryExplorerViewModel _repositoryViewModel = new RepositoryExplorerViewModel();
        public RepositoryExplorerViewModel RepositoryExplorerViewModel { get { return _repositoryViewModel; } }
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
        //public RelayCommand AddNewTreeRepositoryCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(obj =>
        //        {
        //            var repository = new TreeRepository(Guid.Empty);
        //            _treeRepositories.Add(repository);
        //            OnPropertyChanged(nameof(TreeRepositories));
        //            _currentTreeRepository = TreeRepositories.Last();
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
        //            _treeRepositories = service.GetRepositories() ?? new List<TreeRepository>();
        //            OnPropertyChanged(nameof(TreeRepositories));
        //        });
        //    }
        //}
        #endregion
    }
}
