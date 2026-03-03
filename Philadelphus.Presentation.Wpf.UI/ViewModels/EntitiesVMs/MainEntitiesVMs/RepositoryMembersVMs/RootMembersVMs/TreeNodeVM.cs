using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    public class TreeNodeVM : MainEntityBaseVM  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;
        private readonly TreeNodeModel _model;

        public string Alias
        {
            get
            {
                return _model.Alias;
            }
            set
            {
                _model.Alias = value;
                OnPropertyChanged(nameof(Alias));
                OnPropertyChanged(nameof(State));
            }
        }

        public string CustomCode
        {
            get
            {
                return _model.CustomCode;
            }
            set
            {
                _model.CustomCode = value;
                OnPropertyChanged(nameof(CustomCode));
                OnPropertyChanged(nameof(State));
            }
        }

        private readonly ObservableCollection<TreeNodeVM> _childNodes = new ObservableCollection<TreeNodeVM>();
        public ObservableCollection<TreeNodeVM> ChildNodes { get => _childNodes; }

        private readonly ObservableCollection<TreeLeaveVM> _childLeaves = new ObservableCollection<TreeLeaveVM>();
        public ObservableCollection<TreeLeaveVM> ChildLeaves { get => _childLeaves; }
        public CompositeCollection Childs { get; }

        #endregion

        #region [ Construct ]

        public TreeNodeVM(
            TreeNodeModel treeNode,
            IPhiladelphusRepositoryService service) 
            : base(treeNode,  service)
        {
            _service = service;

            _model = treeNode;
            foreach (var item in treeNode.ChildNodes)
            {
                _childNodes.Add(new TreeNodeVM((TreeNodeModel)item, _service));
            }
            foreach (var item in treeNode.ChildLeaves)
            {
                _childLeaves.Add(new TreeLeaveVM((TreeLeaveModel)item, _service));
            }

            Childs = new CompositeCollection()
            {
                new CollectionContainer { Collection = _childNodes },
                new CollectionContainer { Collection = _childLeaves },
            };
        }

        #endregion

        #region [Commands]


        #endregion

        #region [ Methods ]

        public TreeNodeVM CreateTreeNode()
        {
            var resultModel = _service.CreateTreeNode(_model);
            if (resultModel == null)
                return null;
            var result = new TreeNodeVM(resultModel, _service);
            _childNodes.Add(result);
            OnPropertyChanged(nameof(ChildNodes));
            return result;
        }

        public TreeLeaveVM CreateTreeLeave()
        {
            var resultModel = _service.CreateTreeLeave(_model);
            if (resultModel == null)
                return null;
            var result = new TreeLeaveVM(resultModel, _service);
            _childLeaves.Add(result);
            OnPropertyChanged(nameof(ChildLeaves));
            return result;
        }

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
            //for (int i = 0; i < ChildNodes.Count; i++)
            //{
            //    ChildNodes[i].NotifyChildsPropertyChangedRecursive();
            //}
            //for (int i = 0; i < ChildLeaves.Count; i++)
            //{
            //    ChildLeaves[i].NotifyChildsPropertyChangedRecursive();
            //}
            foreach (var item in ChildNodes)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
            foreach (var item in ChildLeaves)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
            foreach (var item in AttributesVMs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion
    }
}
