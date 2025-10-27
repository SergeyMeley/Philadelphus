using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels
{
    public  class TreeRepositoryVM : ViewModelBase
    {
        #region [ Props ]

        private readonly TreeRepositoryService _service;

        private readonly TreeRepositoryModel _model;
        //public TreeRepositoryModel TreeRepository 
        //{ 
        //    get
        //    {
        //        return _model;
        //    }
        //}

        public Guid Guid { get => _model.Guid; }
        public string Name { get => _model.Name; set => _model.Name = value; }
        public string Description { get => _model.Description; set => _model.Description = value; }
        public AuditInfoModel AuditInfo { get => _model.AuditInfo; }
        public State State { get => _model.State; }
        public IDataStorageModel OwnDataStorage { get => _model.OwnDataStorage; }

        private ObservableCollection<IDataStorageModel> _dataStorages { get; }
        public ObservableCollection<IDataStorageModel> DataStorages { get => _dataStorages; }

        public ObservableCollection<TreeRootVM> _childs = new ObservableCollection<TreeRootVM>();
        public ObservableCollection<TreeRootVM> Childs { get => _childs; }

        //public List<TreeRepositoryMemberBaseModel> ElementsCollection { get; internal set; } = new List<TreeRepositoryMemberBaseModel>();

        private MainEntityBaseVM? _selectedRepositoryMember;
        public MainEntityBaseVM? SelectedRepositoryMember
        {
            get => _selectedRepositoryMember;
            set
            {
                _selectedRepositoryMember = value;
                OnPropertyChanged(nameof(PropertyList));
                OnPropertyChanged(nameof(SelectedRepositoryMember));
            }
        }
        //public List<InfrastructureTypes> InfrastructureTypes
        //{
        //    get
        //    {
        //        return Enum.GetValues(typeof(InfrastructureTypes)).Cast<InfrastructureTypes>().ToList();
        //    }
        //}
        public Dictionary<string, string>? PropertyList
        {
            get
            {
                if (_selectedRepositoryMember == null)
                    return null;
                return PropertyGridHelper.GetProperties(_selectedRepositoryMember);
            }
        }

        //private List<string> _visibilityList = new List<string> { "Скрытый (private)", "Всем (public)", "Только наследникам (protected)", "Только элементам корня (internal)" };
        //public List<string> VisibilityList
        //{
        //    get { return _visibilityList; }
        //}

        public string ChildsCount { get => $"Детей: {_model.Childs.Count()}, Корней: {_model?.ChildTreeRoots?.Count()}, Uuids: {_model?.ChildsGuids?.Count}"; }

        public bool IsFavorite 
        { 
            get
            {
                return _model.IsFavorite;
            }
            set
            {
                _model.IsFavorite = value;
            }
        }

        public DateTime? LastOpening
        { 
            get
            {
                return _model.LastOpening;
            }
            set
            {
                _model.LastOpening = value;
            }
        }

        #endregion

        #region [ Construct ]

        public TreeRepositoryVM(TreeRepositoryModel treeRepository)
        {
            _service = new TreeRepositoryService(treeRepository);
            _model = treeRepository;
            foreach (var item in treeRepository.Childs)
            {
                if (item.GetType() == typeof(TreeRootModel))
                {
                    _childs.Add(new TreeRootVM((TreeRootModel)item, _service));
                }
            }
        }

        #endregion

        #region [Commands]
        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _service.SaveChanges(_model);
                    OnPropertyChanged(nameof(State));

                    //TODO: Переработать, временный костыль для проверки
                    //OnPropertyChanged(nameof(Childs));
                    //foreach (var item in Childs)
                    //{
                    //    item.OnPropertyChanged();
                    //    item.OnPropertyChanged(nameof(State));
                    //    foreach (var item2 in item.ChildNodes)
                    //    {
                    //        item2.OnPropertyChanged();
                    //        item2.OnPropertyChanged(nameof(State));
                    //        foreach (var item3 in item2.ChildNodes)
                    //        {
                    //            item3.OnPropertyChanged();
                    //            item3.OnPropertyChanged(nameof(State));
                    //        }
                    //        foreach (var item3 in item2.ChildLeaves)
                    //        {
                    //            item3.OnPropertyChanged();
                    //            item3.OnPropertyChanged(nameof(State));
                    //        }
                    //    }
                    //}
                     NotifyChildsPropertyChangedRecursive();
                });
            }
        }
        public RelayCommand CreateRootCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var result = _service.CreateTreeRoot(_model, _model.OwnDataStorage);
                    Childs.Add(new TreeRootVM(result, _service));
                    OnPropertyChanged(nameof(Childs));
                    OnPropertyChanged(nameof(State));
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
                    if (_selectedRepositoryMember?.GetType() == typeof(TreeRootVM))
                    {
                        ((TreeRootVM)_selectedRepositoryMember).CreateTreeNode();
                    }
                    else if (_selectedRepositoryMember?.GetType() == typeof(TreeNodeVM))
                    {
                        ((TreeNodeVM)_selectedRepositoryMember).CreateTreeNode();
                    }
                    OnPropertyChanged(nameof(Childs));
                    OnPropertyChanged(nameof(State));
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
                    if (_selectedRepositoryMember?.GetType() == typeof(TreeNodeVM))
                    {
                        ((TreeNodeVM)_selectedRepositoryMember).CreateTreeLeave();
                    }
                    OnPropertyChanged(nameof(Childs));
                    OnPropertyChanged(nameof(State));
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
                    //_service.CreateElementAttribute(_selectedRepositoryMember);
                    OnPropertyChanged(nameof(State));
                });
            }
        }
        public RelayCommand DeleteElementCommand
        {
            get 
            {
                return new RelayCommand(obj =>
                {
                    //_service.RemoveMember(_selectedRepositoryMember);
                    OnPropertyChanged(nameof(State));
                });
            }
        }
        #endregion

        #region [ Methods ]

        internal bool LoadTreeRepository()
        {
            _service.GetRepositoryContent(_model);
            Childs.Clear();
            foreach (var item in _model.Childs)
            {
                Childs.Add(new TreeRootVM((TreeRootModel)item, _service));
            }
            OnPropertyChanged(nameof(Childs));
            OnPropertyChanged(nameof(ChildsCount));
            return Childs != null;
        }
        public bool CheckTreeRepositoryAvailability()
        {
            if (_model == null)
                return false;
            return _model.OwnDataStorage.IsAvailable;
        }

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
            foreach (var item in Childs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion
    }
}
