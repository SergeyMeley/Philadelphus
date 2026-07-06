using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs
{
    /// <summary>
    /// Модель представления для корня рабочего дерева.
    /// </summary>
    public class TreeRootVM : MainEntityBaseVM<TreeRootModel>, INodeParent  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;

        private readonly ObservableCollection<TreeNodeVM> _childNodes = new ObservableCollection<TreeNodeVM>();

        public string Alias
        {
            get => _model.Alias;
            set
            {
                _model.Alias = value;
                OnPropertyChanged(nameof(Alias));
                NotifyStateVisibilityPropertiesChanged();
            }
        }

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
                    NotifyStateVisibilityPropertiesChanged();
                }
            }
        }

        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        public ObservableCollection<TreeNodeVM> ChildNodes { get => _childNodes; }
      
        /// <summary>
        /// Дочерние элементы.
        /// </summary>
        public ObservableCollection<TreeNodeVM> Childs => _childNodes;

        public override IEnumerable<IMainEntityVM> TreeChilds => _childNodes;

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeRootVM" />.
        /// </summary>
        /// <param name="treeRoot">Корень рабочего дерева.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeRootVM(
            TreeRootModel treeRoot,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService) 
            : base(treeRoot, dataStoragesCollectionVM, service, fileDialogService, notificationService)
        {
            ArgumentNullException.ThrowIfNull(treeRoot);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(fileDialogService);

            _service = service;

            if (treeRoot.Childs != null)
            {
                foreach (var item in treeRoot.Childs.Values)
                {
                    if (item is TreeNodeModel)
                    {
                        _childNodes.Add(new TreeNodeVM((TreeNodeModel)item, dataStoragesCollectionVM, _service, _fileDialogService, _notificationService));
                    }
                }
            }
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
            var resultModel = _service.CreateTreeNode(_model as TreeRootModel);
            if (resultModel == null)
                return null;
            var result = new TreeNodeVM(resultModel, _dataStoragesCollectionVM, _service, _fileDialogService, _notificationService);
            _childNodes.Add(result);
            OnPropertyChanged(nameof(ChildNodes));
            return result;
        }

        public void NotifyChildsPropertyChangedRecursive()
        {
            NotifyStateVisibilityPropertiesChanged();
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