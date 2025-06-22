using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
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
        private TreeRepository _treeRepository;
        public TreeRepository TreeRepository 
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
        private TreeRepositoryMemberBase? _selectedRepositoryMember;
        public TreeRepositoryMemberBase? SelectedRepositoryMember
        {
            get => _selectedRepositoryMember;
            set
            {
                _selectedRepositoryMember = value;
                _selectedRepositoryMemberProperties = GetProperties(_selectedRepositoryMember);
                OnPropertyChanged(nameof(CurrentTreeElementProperties));
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
        private Dictionary<string, string>? _selectedRepositoryMemberProperties;
        public Dictionary<string, string>? CurrentTreeElementProperties
        {
            get
            {
                if (_selectedRepositoryMember != null)
                {
                    _selectedRepositoryMemberProperties = GetProperties(_selectedRepositoryMember);
                }
                //OnPropertyChanged(nameof(CurrentTreeElementProperties));
                return _selectedRepositoryMemberProperties;
            }
        }
        private List<string> _visabilityList = new List<string> { "Скрытый (private)", "Всем (public)", "Только наследникам (protected)", "Только элементам корня (internal)" };
        public List<string> VisabilityList
        {
            get { return _visabilityList; }
        }

        private Dictionary<string, string> GetProperties(object instance)
        {
            if (instance == null)
                return null;
            var result = new Dictionary<string, string>();
            foreach (var prop in instance.GetType().GetProperties())
            {
                var name = prop.Name;
                var value = string.Empty;
                if (instance != null)
                {

                    //var qwe2 = prop.PropertyType.GetInterfaces();
                    //var qwe4 = qwe2.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    //var qwe5 = qwe2[0].IsGenericType;
                    //var qwe7 = qwe2[0];
                    //var qwe8 = typeof(IEnumerable<>);
                    //var qwe9 = prop.PropertyType.GetInterface("IEnumerable");
                    //var qwe10 = prop.PropertyType.GetInterface("IEnumerable2");
                    //var qwe6 = qwe7 == qwe8;

                    //                var condition3 = prop.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    //                var condition = prop.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    //                var condition2 = Array.Exists(
                    //prop.GetType().GetInterfaces(),
                    //i => i.IsGenericType
                    //  && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));



                    //if (prop.GetType().GetInterfaces().Contains(typeof(IEnumerable<IMainEntity>)) == null)
                    //{
                    //    value = prop.GetValue(instance)?.ToString();
                    //}
                    //else
                    //{
                    //    value = string.Join(",", prop.GetValue(instance));
                    //}

                    value = prop.GetValue(instance)?.ToString();
                }
                result.Add(name, value);
            }
            return result;
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
                        NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevel.Error);
                        return;
                    }
                    if (_selectedRepositoryMember?.GetType().IsAssignableTo(typeof(IParent)) == false)
                        return;
                    var service = new DataTreeRepositoryService();
                    service.InitTreeNode((IParent)_selectedRepositoryMember);
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
                        NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevel.Error);
                        return;
                    }
                    if (_selectedRepositoryMember.GetType().IsAssignableTo(typeof(IParent)) == false)
                    {
                        NotificationService.SendNotification("Лист можно добавить только в элемент, который может быть родителем.", NotificationCriticalLevel.Error, NotificationTypes.TextMessage);
                        return;
                    }
                    var service = new DataTreeRepositoryService();
                    service.InitTreeLeave((IParent)_selectedRepositoryMember);
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
                        NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevel.Error);
                        return;
                    }
                    if (_selectedRepositoryMember.GetType().IsAssignableTo(typeof(IContentOwner)) == false)
                        return;
                    var service = new DataTreeRepositoryService();
                    service.InitElementAttribute((IContentOwner)_selectedRepositoryMember);
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
