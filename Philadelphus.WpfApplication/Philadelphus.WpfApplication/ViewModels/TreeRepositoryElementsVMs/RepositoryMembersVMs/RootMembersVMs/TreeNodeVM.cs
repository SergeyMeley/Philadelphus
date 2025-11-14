using Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Philadelphus.WpfApplication.ViewModels.TreeRepositoryElementsVMs.RepositoryMembersVMs.RootMembersVMs
{
    public class TreeNodeVM : MainEntityBaseVM
    {
        #region [ Props ]

        private readonly TreeNodeModel _model;

        private readonly ObservableCollection<TreeNodeVM> _childNodes = new ObservableCollection<TreeNodeVM>();
        public ObservableCollection<TreeNodeVM> ChildNodes { get => _childNodes; }

        private readonly ObservableCollection<TreeLeaveVM> _childLeaves = new ObservableCollection<TreeLeaveVM>();
        public ObservableCollection<TreeLeaveVM> ChildLeaves { get => _childLeaves; }
        public CompositeCollection Childs { get; }

        #endregion

        #region [ Construct ]

        public TreeNodeVM(TreeNodeModel treeNode, TreeRepositoryService service) : base(treeNode, service)
        {
            _model = treeNode;
            foreach (var item in treeNode.Childs)
            {
                if (item.GetType() == typeof(TreeNodeModel))
                {
                    _childNodes.Add(new TreeNodeVM((TreeNodeModel)item, service));
                }
                else if (item.GetType() == typeof(TreeLeaveModel))
                {
                    _childLeaves.Add(new TreeLeaveVM((TreeLeaveModel)item, service));
                }
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
            foreach (var item in PersonalAttributesVMs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion
    }
}
