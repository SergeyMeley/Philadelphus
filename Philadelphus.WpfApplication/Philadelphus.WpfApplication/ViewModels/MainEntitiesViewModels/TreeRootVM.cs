using Microsoft.VisualBasic;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels
{
    public class TreeRootVM : MainEntityBaseVM
    {
        #region [ Props ]

        private readonly TreeRootModel _model;

        private readonly ObservableCollection<TreeNodeVM> _childNodes = new ObservableCollection<TreeNodeVM>();
        public ObservableCollection<TreeNodeVM> ChildNodes { get => _childNodes; }
        public CompositeCollection Childs { get; }

        #endregion

        #region [ Construct ]

        public TreeRootVM(TreeRootModel treeRoot, TreeRepositoryService service) : base(treeRoot, service)
        {
            _model = treeRoot;
            if (treeRoot.Childs != null)
            {
                foreach (var item in treeRoot.Childs)
                {
                    if (item.GetType() == typeof(TreeNodeModel))
                    {
                        _childNodes.Add(new TreeNodeVM((TreeNodeModel)item, service));
                    }
                }
            }
            Childs = new CompositeCollection()
            {
                new CollectionContainer { Collection = _childNodes },
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

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
            //for (int i = 0; i < ChildNodes.Count; i++)
            //{
            //    ChildNodes[i].NotifyChildsPropertyChangedRecursive();
            //}
            foreach (var item in ChildNodes)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion
    }
}
