using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    public class TreeRootVM : MainEntityBaseVM  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;

        private readonly ObservableCollection<TreeNodeVM> _childNodes = new ObservableCollection<TreeNodeVM>();

        public string CustomCode
        {
            get
            {
                if (_model is TreeRootModel m)
                {
                    return m.CustomCode;
                }
                return string.Empty;
            }
            set
            {
                if (_model is TreeRootModel m)
                {
                    m.CustomCode = value;
                    OnPropertyChanged(nameof(CustomCode));
                    OnPropertyChanged(nameof(State));
                }
            }
        }
        public ObservableCollection<TreeNodeVM> ChildNodes { get => _childNodes; }
        public CompositeCollection Childs { get; }

        #endregion

        #region [ Construct ]

        public TreeRootVM(
            TreeRootModel treeRoot,
            IPhiladelphusRepositoryService service) 
            : base(treeRoot, service)
        {
            _service = service;

            if (treeRoot.Childs != null)
            {
                foreach (var item in treeRoot.Childs.Values)
                {
                    if (item is TreeNodeModel)
                    {
                        _childNodes.Add(new TreeNodeVM((TreeNodeModel)item, _service));
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
            var resultModel = _service.CreateTreeNode(_model as TreeRootModel);
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
            foreach (var item in AttributesVMs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion
    }
}
