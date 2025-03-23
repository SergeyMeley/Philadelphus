using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {
        private List<TreeRepository> _treeRepositories;
        public List<TreeRepository> TreeRepositories
        {
            get
            {
                if (_treeRepositories == null)
                {
                    _treeRepositories = new List<TreeRepository>();
                    for (int i = 0; i < 5; i++)
                    {
                        
                        //TreeRepository treeRepository = new TreeRepository(Guid.NewGuid());
                        //Временно
                        DataTreeRepositoryService treeRepositoryService = new DataTreeRepositoryService();
                        TreeRepository treeRepository = treeRepositoryService.CreateSampleRepository();
                        //Временно
                        treeRepository.Name = $"TEST {i}";
                        _treeRepositories.Add(treeRepository);
                    }
                }
                return _treeRepositories;
            }
            private set
            {
                _treeRepositories = value.ToList();
                OnPropertyChanged(nameof(TreeRepositories));
            }
        }
        private static TreeRepository _currentTreeRepository;
        public TreeRepository CurrentTreeRepository
        {
            get
            {
                return _currentTreeRepository;
            }
            set
            {
                _currentTreeRepository = value;
                _repositoryViewModel.CurrentTreeRepository = _currentTreeRepository;
                OnPropertyChanged(nameof(CurrentTreeRepository));
            }
        }
        private RepositoryExplorerViewModel _repositoryViewModel = new RepositoryExplorerViewModel(_currentTreeRepository);
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
        public RelayCommand AddNewTreeRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var repository = new TreeRepository(Guid.Empty);
                    _treeRepositories.Add(repository);
                    OnPropertyChanged(nameof(TreeRepositories));
                    _currentTreeRepository = TreeRepositories.Last();
                    OnPropertyChanged(nameof(CurrentTreeRepository));
                });
            }
        }
        public RelayCommand SaveRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeRepositoryService();
                    service.ModifyRepository(_currentTreeRepository);
                });
            }
        }
        public RelayCommand RefreshRepositoryListCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeRepositoryService();
                    _treeRepositories = service.GetRepositories() ?? new List<TreeRepository>();
                    OnPropertyChanged(nameof(TreeRepositories));
                });
            }
        }
        #endregion
    }
}
