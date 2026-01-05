using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    public class TreeRootVM : MainEntityBaseVM  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly ITreeRepositoryService _service;

        private readonly TreeRootModel _model;

        private readonly ObservableCollection<TreeNodeVM> _childNodes = new ObservableCollection<TreeNodeVM>();
        public ObservableCollection<TreeNodeVM> ChildNodes { get => _childNodes; }
        public CompositeCollection Childs { get; }

        #endregion

        #region [ Construct ]

        public TreeRootVM(
            TreeRootModel treeRoot,
            ITreeRepositoryService service) 
            : base(treeRoot, service)
        {
                _service = service;

            _model = treeRoot;
            if (treeRoot.Childs != null)
            {
                foreach (var item in treeRoot.Childs)
                {
                    if (item.GetType() == typeof(TreeNodeModel))
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
            foreach (var item in PersonalAttributesVMs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        #endregion
    }
}
