using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    /// <summary>
    /// Модель представления для узла рабочего дерева.
    /// </summary>
    public class TreeNodeVM : MainEntityBaseVM<TreeNodeModel>, INodeParent, ILeaveParent  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
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
        
        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        public ObservableCollection<TreeNodeVM> ChildNodes { get => _childNodes; }

        private readonly ObservableCollection<TreeLeaveVM> _childLeaves = new ObservableCollection<TreeLeaveVM>();
        
        /// <summary>
        /// Дочерние листья.
        /// </summary>
        public ObservableCollection<TreeLeaveVM> ChildLeaves { get => _childLeaves; }
       
        /// <summary>
        /// Дочерние элементы.
        /// </summary>
        public CompositeCollection Childs { get; }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeNodeVM" />.
        /// </summary>
        /// <param name="treeNode">Узел рабочего дерева.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeNodeVM(
            TreeNodeModel treeNode,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service) 
            : base(treeNode, dataStoragesCollectionVM, service)
        {
            ArgumentNullException.ThrowIfNull(treeNode);
            ArgumentNullException.ThrowIfNull(treeNode.ChildNodes);
            ArgumentNullException.ThrowIfNull(treeNode.ChildLeaves);
            ArgumentNullException.ThrowIfNull(service);

            _service = service;

            _model = treeNode;
            foreach (var item in treeNode.ChildNodes)
            {
                _childNodes.Add(new TreeNodeVM(item, dataStoragesCollectionVM, _service));
            }
            foreach (var item in treeNode.ChildLeaves)
            {
                _childLeaves.Add(new TreeLeaveVM(this, item, dataStoragesCollectionVM, _service));
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

        /// <summary>
        /// Создает объект узла рабочего дерева.
        /// </summary>
        /// <returns>Созданный объект.</returns>
        public TreeNodeVM CreateTreeNode()
        {
            var resultModel = _service.CreateTreeNode(_model);
            if (resultModel == null)
                return null;
            var result = new TreeNodeVM(resultModel, _dataStoragesCollectionVM, _service);
            _childNodes.Add(result);
            OnPropertyChanged(nameof(ChildNodes));
            return result;
        }

        /// <summary>
        /// Создает объект листа рабочего дерева.
        /// </summary>
        /// <returns>Созданный объект.</returns>
        public TreeLeaveVM CreateTreeLeave()
        {
            var resultModel = _service.CreateTreeLeave(_model);
            if (resultModel == null)
                return null;
            var result = new TreeLeaveVM(this, resultModel, _dataStoragesCollectionVM, _service);
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
