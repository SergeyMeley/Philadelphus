using Philadelphus.Business.Entities.ElementsProperties;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.WpfApplication.ViewModels.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs
{
    public  class TreeRepositoryVM : ViewModelBase
    {
        #region [ Props ]

        private readonly TreeRepositoryService _service;

        private readonly TreeRepositoryModel _model;
        public TreeRepositoryModel TreeRepositoryModel
        {
            get
            {
                return _model;
            }
        }

        public Guid Guid { get => _model.Guid; }
        public string Name { get => _model.Name; set => _model.Name = value; }
        public string Description { get => _model.Description; set => _model.Description = value; }
        public AuditInfoModel AuditInfo { get => _model.AuditInfo; }
        public State State { get => _model.State; }
        public IDataStorageModel OwnDataStorage { get => _model.OwnDataStorage; }

        private DataStorageVM _storageVM;
        public DataStorageVM StorageVM { get => _storageVM; }
        public string OwnDataStorageName
        {
            get
            {
                return _model.OwnDataStorageName;
            }
        }
        public Guid OwnDataStorageUuid
        {
            get
            {
                return _model.OwnDataStorageUuid;
            }
        }

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

        public TreeRepositoryVM(TreeRepositoryModel treeRepositoryModel)
        {
            _service = new TreeRepositoryService(treeRepositoryModel);
            _model = treeRepositoryModel;
            _storageVM = new DataStorageVM(treeRepositoryModel.OwnDataStorage);
            foreach (var item in treeRepositoryModel.Childs)
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
                    UpdateChildsCollection(this);
                    _service.SaveChanges(_model);
                    OnPropertyChanged(nameof(State));
                    NotifyChildsPropertyChangedRecursive();
                });
            }
        }
        private bool UpdateChildsCollection(ViewModelBase parent)    //TODO: Переделать временный костыль
        {
            if (parent is TreeRepositoryVM)
            {
                var repository = (TreeRepositoryVM)parent;
                for (int i = repository.Childs.Count - 1; i >= 0; i--)
                {
                    if (repository.Childs[i].State == State.ForHardDelete
                    || repository.Childs[i].State == State.ForSoftDelete
                    || repository.Childs[i].State == State.SoftDeleted)
                    {
                        repository.Childs.Remove(repository.Childs[i]);
                    }
                    else
                        UpdateChildsCollection(repository.Childs[i]);
                }
            }
            if (parent is TreeRootVM)
            {
                var root = (TreeRootVM)parent;
                for (int i = root.ChildNodes.Count - 1; i >= 0; i--)
                {
                    if (root.ChildNodes[i].State == State.ForHardDelete
                    || root.ChildNodes[i].State == State.ForSoftDelete
                    || root.ChildNodes[i].State == State.SoftDeleted)
                    {
                        root.ChildNodes.Remove(root.ChildNodes[i]);
                    }
                    else
                        UpdateChildsCollection(root.ChildNodes[i]);
                }
            }
            if (parent is TreeNodeVM)
            {
                var node = (TreeNodeVM)parent;
                for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                {
                    if (node.ChildNodes[i].State == State.ForHardDelete
                    || node.ChildNodes[i].State == State.ForSoftDelete
                    || node.ChildNodes[i].State == State.SoftDeleted)
                    {
                        node.ChildNodes.Remove(node.ChildNodes[i]);
                    }
                    else
                        UpdateChildsCollection(node.ChildNodes[i]);
                }
                for (int i = node.ChildLeaves.Count - 1; i >= 0; i--)
                {
                    if (node.ChildLeaves[i].State == State.ForHardDelete
                    || node.ChildLeaves[i].State == State.ForSoftDelete
                    || node.ChildLeaves[i].State == State.SoftDeleted)
                    {
                        node.ChildLeaves.Remove(node.ChildLeaves[i]);
                    }
                    else
                        UpdateChildsCollection(node.ChildLeaves[i]);
                }
            }
            return true;
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
                    _selectedRepositoryMember.AddAttribute();
                    
                    OnPropertyChanged(nameof(State));
                });
            }
        }
        public RelayCommand SoftDeleteRepositoryMemberCommand
        {
            get 
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember.Model is IChildrenModel)
                    {
                        var member = (IChildrenModel)_selectedRepositoryMember.Model;
                        _service.SoftDeleteRepositoryMember(member);
                    }
                    OnPropertyChanged(nameof(State));
                    NotifyChildsPropertyChangedRecursive();
                },
                ce =>
                {
                    return _selectedRepositoryMember?.Model is IChildrenModel;
                });
            }
        }
        public RelayCommand ProtectCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                },
                ce =>
                {
                    return false;
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
