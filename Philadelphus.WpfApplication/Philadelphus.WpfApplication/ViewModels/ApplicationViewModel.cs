using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    internal class ApplicationViewModel : ViewModelBase
    {
        public List<InfrastructureRepositoryTypes> InfrastructureRepositories
        {
            get
            {
               return Enum.GetValues(typeof(InfrastructureRepositoryTypes)).Cast<InfrastructureRepositoryTypes>().ToList();
            }
        }
        private TreeRepository _selectedTreeRepository;
        public TreeRepository SelectedTreeRepository 
        { 
            get => _selectedTreeRepository; 
            set
            {
                _selectedTreeRepository = value; 
                OnPropertyChanged(nameof(SelectedTreeRepository));
            }
        }
        private List<TreeRepository> _treeRepositories = new List<TreeRepository>();
        public ObservableCollection<TreeRepository> TreeRepositories { get => new ObservableCollection<TreeRepository>(_treeRepositories); private set => _treeRepositories = value.ToList(); }
        public RelayCommand AddNewTreeRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var repository = new TreeRepository("Новый репозиторий", Guid.Empty);
                    _treeRepositories.Add(repository);
                    OnPropertyChanged(nameof(TreeRepositories));
                    _selectedTreeRepository = TreeRepositories.Last();
                    OnPropertyChanged(nameof(SelectedTreeRepository));
                });
            }
        }
        public RelayCommand SaveRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeRepositoryService();
                    service.ModifyRepository(_selectedTreeRepository);
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
                    _treeRepositories = service.GetRepositoryList() ?? new List<TreeRepository>();
                    OnPropertyChanged(nameof(TreeRepositories));
                });
            }
        }
    }
}
