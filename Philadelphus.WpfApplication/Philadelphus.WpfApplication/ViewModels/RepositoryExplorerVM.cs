using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public  class RepositoryExplorerVM : ViewModelBase
    {
        private TreeRepositoryModel _treeRepository;
        public TreeRepositoryModel TreeRepository { get => _treeRepository; }

        private TreeRepositoryMemberBaseModel? _selectedRepositoryMember;
        public TreeRepositoryMemberBaseModel? SelectedRepositoryMember
        {
            get => _selectedRepositoryMember;
            set
            {
                _selectedRepositoryMember = value;
                OnPropertyChanged(nameof(PropertyList));
                OnPropertyChanged(nameof(SelectedRepositoryMember));
            }
        }
        public List<InfrastructureTypes> InfrastructureTypes
        {
            get
            {
                return Enum.GetValues(typeof(InfrastructureTypes)).Cast<InfrastructureTypes>().ToList();
            }
        }
        public Dictionary<string, string>? PropertyList
        {
            get
            {
                if (_selectedRepositoryMember == null)
                    return null;
                return PropertyGridHelper.GetProperties(_selectedRepositoryMember);
            }
        }

        private List<string> _visibilityList = new List<string> { "Скрытый (private)", "Всем (public)", "Только наследникам (protected)", "Только элементам корня (internal)" };
        public List<string> VisibilityList
        {
            get { return _visibilityList; }
        }
        public RepositoryExplorerVM(TreeRepositoryModel treeRepository)
        {
            _treeRepository = treeRepository;
        }
        public bool CheckTreeRepositoryAvailability()
        {
            if (_treeRepository == null)
                return false;
            return _treeRepository.OwnDataStorage.IsAvailable;
        }


        #region [Commands]
        public RelayCommand CreateRootCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    
                    var service = new DataTreeProcessingService();
                    service.CreateTreeRoot(_treeRepository, null);
                });
            }
        }
        public RelayCommand CreateNodeCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember == null)
                    {
                        NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevelModel.Error);
                        return;
                    }
                    if (_selectedRepositoryMember?.GetType().IsAssignableTo(typeof(IParentModel)) == false)
                        return;
                    var service = new DataTreeProcessingService();
                    service.CreateTreeNode((IParentModel)_selectedRepositoryMember);
                });
            }
        }
        public RelayCommand CreateLeaveCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember == null)
                    {
                        NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevelModel.Error);
                        return;
                    }
                    if (_selectedRepositoryMember.GetType().IsAssignableTo(typeof(IParentModel)) == false)
                    {
                        NotificationService.SendNotification("Лист можно добавить только в элемент, который может быть родителем.", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
                        return;
                    }
                    var service = new DataTreeProcessingService();
                    service.CreateTreeLeave((IParentModel)_selectedRepositoryMember);
                });
            }
        }
        public RelayCommand CreateAttributeCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember == null)
                    {
                        NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevelModel.Error);
                        return;
                    }
                    if (_selectedRepositoryMember.GetType().IsAssignableTo(typeof(IContentOwnerModel)) == false)
                        return;
                    var service = new DataTreeProcessingService();
                    service.CreateElementAttribute((IContentOwnerModel)_selectedRepositoryMember);
                });
            }
        }
        public RelayCommand DeleteElementCommand
        {
            get 
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeProcessingService();
                    service.RemoveElement(_selectedRepositoryMember);
                });
            }
        }
        #endregion
    }
}
