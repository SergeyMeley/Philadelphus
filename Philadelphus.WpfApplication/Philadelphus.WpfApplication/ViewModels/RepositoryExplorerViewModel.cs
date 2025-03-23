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
    public  class RepositoryExplorerViewModel : ViewModelBase
    {
        public RepositoryExplorerViewModel(TreeRepository treeRepository)
        {
            _currentTreeRepository = treeRepository;
            //((List<ViewModelBase>)Cache).Add(new RepositoryExplorerViewModel());
        }
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
                OnPropertyChanged(nameof(CurrentTreeRepository));
            }
        }
        #region [Commands]
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
