using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers
{
    public class WorkingTreeModel : ShrubMemberBaseModel, IPhiladelphusRepositoryMemberModel, IHavingOwnDataStorageModel, IOwnerModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новое рабочее дерево";

        private IDataStorageModel _ownDataStorage;
        private TreeRootModel _contentRoot;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Системное рабочее дерево
        /// </summary>
        public bool IsSystemBase { get => Uuid == SystemBaseUuid; }

        /// <summary>
        /// Уникальный идентификатор системного рабочего дерева
        /// </summary>
        internal static Guid SystemBaseUuid { get => Guid.Parse("00000000-0000-0000-0000-0000002018ee"); }

        /// <summary>
        /// Недоступные (занятые) имена
        /// </summary>
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
        /// <param name="uuid"></param>
        /// <param name="dataStorage"></param>
        /// <param name="dbEntity"></param>
        /// <param name="owner"></param>
        internal WorkingTreeModel(
            Guid uuid,
            IDataStorageModel dataStorage,
            ShrubModel owner)
            : base(uuid, owner)
        {
            if (dataStorage == null)
                throw new ArgumentNullException(nameof(dataStorage));

            _ownDataStorage = dataStorage;

            Name = NamingHelper.GetNewName(UnavailableNames, _defaultFixedPartOfName);
            UnavailableNames.Add(Name);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Изменить хранилище данных (не реализовано)
        /// </summary>
        /// <param name="storage">Новое хранилище</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получить все узлы рабочего дерева (рекурсивно)
        /// </summary>
        public IEnumerable<TreeNodeModel> GetAllNodesRecursive()
        {
            return ContentRoot?.GetAllNodesRecursive() ?? throw new NullReferenceException();
        }

        /// <summary>
        /// Получить все листы рабочего дерева (рекурсивно)
        /// </summary>
        public IEnumerable<TreeLeaveModel> GetAllLeavesRecursive()
        {
            return ContentRoot?.GetAllLeavesRecursive() ?? throw new NullReferenceException();
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
        protected override bool ClearContentDetailed()
        {
            ContentRoot = null;
            return true;
        }

        #endregion
    }
}
