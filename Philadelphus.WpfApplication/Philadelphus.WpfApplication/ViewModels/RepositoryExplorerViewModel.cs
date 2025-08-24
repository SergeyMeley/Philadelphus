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
    public  class RepositoryExplorerViewModel : ViewModelBase
    {
        private TreeRepositoryModel _treeRepository;
        public TreeRepositoryModel TreeRepository 
        { 
            get 
            { 
                return _treeRepository; 
            } 
            set
            {
                _treeRepository = value;
            }
        }
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
        private List<string> _visabilityList = new List<string> { "Скрытый (private)", "Всем (public)", "Только наследникам (protected)", "Только элементам корня (internal)" };
        public List<string> VisabilityList
        {
            get { return _visabilityList; }
        }

        #region [Commands]
        public RelayCommand CreateRootCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    
                    var service = new DataTreeRepositoryService();
                    service.InitTreeRoot(_treeRepository);
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
                    var service = new DataTreeRepositoryService();
                    service.InitTreeNode((IParentModel)_selectedRepositoryMember);
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
                    var service = new DataTreeRepositoryService();
                    service.InitTreeLeave((IParentModel)_selectedRepositoryMember);
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
                    var service = new DataTreeRepositoryService();
                    service.InitElementAttribute((IContentOwnerModel)_selectedRepositoryMember);
                });
            }
        }

        public RelayCommand DeleteElementCommand
        {
            get 
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeRepositoryService();
                    service.RemoveElement(_selectedRepositoryMember);
                });
            }
        }
        #endregion
    }
}
