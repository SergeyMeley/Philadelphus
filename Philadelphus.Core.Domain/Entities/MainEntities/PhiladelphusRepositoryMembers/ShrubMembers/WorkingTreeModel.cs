using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers
{
    /// <summary>
    /// Доменная модель рабочего дерева.
    /// </summary>
    public class WorkingTreeModel : ShrubMemberBaseModel<WorkingTreeModel>, IPhiladelphusRepositoryMemberModel, IHavingOwnDataStorageModel, IOwnerModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новое рабочее дерево";

        private IDataStorageModel _ownDataStorage;
        private TreeRootModel _contentRoot;
        private ICollection<TreeNodeModel> _contentNodes = new List<TreeNodeModel>();
        private ICollection<TreeLeaveModel> _contentLeaves = new List<TreeLeaveModel>();
        private ICollection<ElementAttributeModel> _contentAttributes = new List<ElementAttributeModel>();

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Системное рабочее дерево
        /// </summary>
        public bool IsSystemBase { get; } = false;

        /// <summary>
        /// Уникальный идентификатор системного рабочего дерева
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        internal static Guid SystemBaseUuid { get => Guid.Parse("00000000-0000-0000-0000-0000002018ee"); }

        /// <summary>
        /// Недоступные (занятые) имена
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public HashSet<string> UnavailableNames { get; } = new HashSet<string>();

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Содержимое
        /// </summary>
        public override ReadOnlyDictionary<Guid, IContentModel> Content
        {
            get
            {
                var result = new Dictionary<Guid, IContentModel>();

                if (ContentRoot == null)
                {
                    return result.AsReadOnly();
                }

                result.Add(ContentRoot.Uuid, ContentRoot);

                foreach (var node in GetAllNodesRecursive())
                {
                    result.Add(node.Uuid, node);
                }

                foreach (var leave in GetAllLeavesRecursive())
                {
                    result.Add(leave.Uuid, leave);
                }

                return result.AsReadOnly();
            }
            
        }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        public virtual ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => throw new NotImplementedException(); 
        }

        /// <summary>
        /// Корень рабочего дерева
        /// </summary>
        public TreeRootModel ContentRoot
        {
            get
            {
                return _contentRoot;
            }
            set
            {
                if (_contentRoot != value)
                {
                    _contentRoot = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Узлы рабочего дерева
        /// </summary>
        public ICollection<TreeNodeModel> ContentNodes
        {
            get
            {
                return _contentNodes;
            }
            set
            {
                if (_contentNodes != value)
                {
                    _contentNodes = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Листы рабочего дерева
        /// </summary>
        public ICollection<TreeLeaveModel> ContentLeaves 
        {
            get
            {
                return _contentLeaves;
            }
            set
            {
                if (_contentLeaves != value)
                {
                    _contentLeaves = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Атрибуты элементов рабочего дерева
        /// </summary>
        public ICollection<ElementAttributeModel> ContentAttributes
        {
            get
            {
                return _contentAttributes;
            }
            set
            {
                if (_contentAttributes != value)
                {
                    _contentAttributes = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        public IDataStorageModel OwnDataStorage
        {
            get
            {
                return _ownDataStorage;
            }
        }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public sealed override IDataStorageModel DataStorage { get => OwnDataStorage; }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Рабочее дерево
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор.</param>
        /// <param name="dataStorage">Хранилище данных.</param>
        /// <param name="owner">Владелец.</param>
        internal WorkingTreeModel(
            Guid uuid,
            IDataStorageModel dataStorage,
            ShrubModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<WorkingTreeModel> propertiesPolicy)
            : base(uuid, owner, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(dataStorage);

            _ownDataStorage = dataStorage;

            if (uuid == SystemBaseUuid)
            {
                IsSystemBase = true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Изменить хранилище данных (не реализовано)
        /// </summary>
        /// <param name="storage">Новое хранилище</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получить все узлы рабочего дерева (рекурсивно)
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public IEnumerable<TreeNodeModel> GetAllNodesRecursive()
        {
            return ContentRoot?.GetAllNodesRecursive() ?? Enumerable.Empty<TreeNodeModel>();
        }

        /// <summary>
        /// Получить все листы рабочего дерева (рекурсивно)
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public IEnumerable<TreeLeaveModel> GetAllLeavesRecursive()
        {
            return ContentRoot?.GetAllLeavesRecursive() ?? Enumerable.Empty<TreeLeaveModel>();
        }

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        protected override bool AddContentDetailed(IContentModel content)
        {
            if (content is TreeRootModel)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        protected override bool RemoveContentDetailed(IContentModel content)
        {
            if (content is TreeRootModel r
                && ContentRoot.Uuid == content.Uuid)
            {
                ContentRoot = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        protected override bool ClearContentDetailed()
        {
            ContentRoot = null;
            return true;
        }

        #endregion
    }
}
