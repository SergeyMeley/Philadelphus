using AutoMapper;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Business.Config;
using Philadelphus.Business.Entities.ElementsProperties;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.Business.Services.Interfaces;
using Philadelphus.WpfApplication.Factories.Interfaces;
using Philadelphus.WpfApplication.Infrastructure;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels.ControlsVMs
{
    public  class RepositoryExplorerControlVM : ControlVM
    {
        #region [ Props ]

        private readonly ITreeRepositoryService _service;
        private readonly TreeRepositoryVM _treeRepositoryVM;
        public TreeRepositoryVM TreeRepositoryVM 
        { 
            get 
            { 
                return _treeRepositoryVM; 
            }
        }


        public string CurentRepositoryName { get => _treeRepositoryVM.Name; }

        //public List<MainEntityBaseModel> ElementsCollection { get; internal set; } = new List<MainEntityBaseModel>();

        private MainEntityBaseVM? _selectedRepositoryMember;
        public MainEntityBaseVM? SelectedRepositoryMember
        {
            get
            {
                return _selectedRepositoryMember;
            }
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

        private ExtensionsControlVM _extensionsControlVM;
        public ExtensionsControlVM ExtensionsControlVM { get => _extensionsControlVM; }

        #endregion

        #region [ Construct ]

        public RepositoryExplorerControlVM(
            IServiceProvider serviceProvider,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            IOptions<ApplicationSettings> options,
            ITreeRepositoryService service,
            IExtensionsControlVMFactory extensionVMFactory,
            TreeRepositoryVM treeRepositoryVM)
            : base(serviceProvider, logger, notificationService)
        {
            _service = service;
            _extensionsControlVM = extensionVMFactory.Create(this);
            _treeRepositoryVM = treeRepositoryVM;

            LoadTreeRepository();

            _notificationService.SendTextMessage("Обозреватель репозитория. Начало инициализации расширений", NotificationCriticalLevelModel.Info);
            _extensionsControlVM.InitializeAsync(options.Value.PluginsDirectoriesString);
            _notificationService.SendTextMessage($"Обозреватель репозитория. Расширения инициализированы ({ExtensionsControlVM.Extensions?.Count()} шт)", NotificationCriticalLevelModel.Info);
        }

        #endregion

        #region [Commands]
        public RelayCommand GetWorkCommandа
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    LoadTreeRepository();
                });
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    UpdateChildsCollection(this);
                    _service.SaveChanges(_treeRepositoryVM.Model);
                    OnPropertyChanged(nameof(State));
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
                    var result = _service.CreateTreeRoot(_treeRepositoryVM.Model, _treeRepositoryVM.Model.OwnDataStorage);
                    _treeRepositoryVM.Childs.Add(new TreeRootVM(result, _service));
                    OnPropertyChanged(nameof(_treeRepositoryVM.Childs));
                    OnPropertyChanged(nameof(_treeRepositoryVM.State));
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
                        return;
                    if (_selectedRepositoryMember?.GetType() == typeof(TreeRootVM))
                    {
                        ((TreeRootVM)_selectedRepositoryMember).CreateTreeNode();
                    }
                    else if (_selectedRepositoryMember?.GetType() == typeof(TreeNodeVM))
                    {
                        ((TreeNodeVM)_selectedRepositoryMember).CreateTreeNode();
                    }
                    OnPropertyChanged(nameof(_treeRepositoryVM.Childs));
                    OnPropertyChanged(nameof(_treeRepositoryVM.State));
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
                        return;
                    if (_selectedRepositoryMember?.GetType() == typeof(TreeNodeVM))
                    {
                        ((TreeNodeVM)_selectedRepositoryMember).CreateTreeLeave();
                    }
                    OnPropertyChanged(nameof(_treeRepositoryVM.Childs));
                    OnPropertyChanged(nameof(_treeRepositoryVM.State));
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
                        return;
                    _selectedRepositoryMember.AddAttribute();
                    
                    OnPropertyChanged(nameof(_treeRepositoryVM.State));
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
            _service.GetRepositoryContent(_treeRepositoryVM.Model);
            _treeRepositoryVM.Childs.Clear();
            foreach (var item in _treeRepositoryVM.Model.Childs)
            {
                _treeRepositoryVM.Childs.Add(new TreeRootVM((TreeRootModel)item, _service));
            }
            OnPropertyChanged(nameof(_treeRepositoryVM));
            OnPropertyChanged(nameof(_treeRepositoryVM.Childs));
            OnPropertyChanged(nameof(_treeRepositoryVM.ChildsCount));
            return _treeRepositoryVM.Childs != null;
        }
        public bool CheckTreeRepositoryAvailability()
        {
            if (_treeRepositoryVM.Model == null)
                return false;
            return _treeRepositoryVM.Model.OwnDataStorage.IsAvailable;
        }

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
            foreach (var item in _treeRepositoryVM.Childs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion


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
    }
}
