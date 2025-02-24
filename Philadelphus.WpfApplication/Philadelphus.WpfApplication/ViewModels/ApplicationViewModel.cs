using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels
{
    internal class ApplicationViewModel : ViewModelBase
    {
        private IMainEntity _selectedEntity;
        public IMainEntity SelectedEntity 
        {
            get => _selectedEntity;
            set
            {
                _selectedEntity = value;
                OnPropertyChanged(nameof(SelectedEntity));
            }
        }
        public List<InfrastructureRepositoryTypes> InfrastructureRepositories
        {
            get
            {
               return Enum.GetValues(typeof(InfrastructureRepositoryTypes)).Cast<InfrastructureRepositoryTypes>().ToList();
            }
        }
        private TreeRepository _currentTreeRepository;
        public TreeRepository CurrentTreeRepository 
        {
            get
            {
                //Временно
                DataTreeRepositoryService treeRepositoryService = new DataTreeRepositoryService();
                _currentTreeRepository = treeRepositoryService.CreateRepository();
                //Временно
                return _currentTreeRepository;
            }
            set
            {
                _currentTreeRepository = value; 
                OnPropertyChanged(nameof(CurrentTreeRepository));
            }
        }
        private List<TreeRepository> _treeRepositories = new List<TreeRepository>();
        public ObservableCollection<TreeRepository> TreeRepositories { get => new ObservableCollection<TreeRepository>(_treeRepositories); private set => _treeRepositories = value.ToList(); }

        #region [Commands]
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
        public RelayCommand AddRootCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeRepositoryService();
                    ((List<TreeRoot>)_currentTreeRepository.Childs).Add(new TreeRoot(_currentTreeRepository.Guid));
                    OnPropertyChanged(nameof(CurrentTreeRepository));
                });
            }
        }
        #endregion
    }
}
