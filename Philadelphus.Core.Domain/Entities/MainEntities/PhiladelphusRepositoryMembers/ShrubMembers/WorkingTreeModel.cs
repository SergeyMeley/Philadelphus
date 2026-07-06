using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "[Системный]", Description = "Системный элемент")]
        public bool IsSystemBase { get; } = false;

        /// <summary>
        /// Уникальный идентификатор системного рабочего дерева
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        [Display(Name = "[UUID системного элемента]", Description = "UUID системного базового элемента")]
        internal static Guid SystemBaseUuid { get => Guid.Parse("00000000-0000-0000-0000-0000002018ee"); }

        /// <summary>
        /// Недоступные (занятые) наименования
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        [Display(Name = "[Недоступные наименования]", Description = "Недоступные (занятые) наименования")]
        public HashSet<string> UnavailableNames { get; } = new HashSet<string>();

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Содержимое
        /// </summary>
        [Display(Name = "[Содержимое]", Description = "Содержимое")]
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

                foreach (var attribute in ContentAttributes)
                {
                    result.TryAdd(attribute.Uuid, attribute);
                }

                return result.AsReadOnly();
            }
            
        }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        [Display(Name = "[Все содержимое]", Description = "Все содержимое")]
        public override ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => RecursiveRelationshipHelper.ToReadOnlyDictionary(
                RecursiveRelationshipHelper.EnumerateContentRecursive(this)); 
        }

        /// <summary>
        /// Корень рабочего дерева
        /// </summary>
        [Display(Name = "[Корень]", Description = "Содержащийся корень")]
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
        [Display(Name = "[Узлы]", Description = "Содержащиеся узлы")]
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
        [Display(Name = "[Листы]", Description = "Содержащиеся листы")]
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
        [Display(Name = "[Атрибуты]", Description = "Содержащиеся атрибуты")]
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
        [Display(Name = "[Собственное хранилище]", Description = "Собственное хранилище данных")]
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
        [Display(Name = "[Хранилище]", Description = "Хранилище данных")]
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
        /// Добавить атрибут, если рабочее дерево не является системным.
        /// </summary>
        /// <param name="attribute">Атрибут.</param>
        /// <returns>true, если атрибут добавлен; false, если рабочее дерево системное или операция не выполнена.</returns>
        /// <remarks>Implements requirement R-5.05 for the system working tree.</remarks>
        public override bool AddAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (IsSystemBase)
            {
                SendAttributeCollectionRestriction("R-5.05", "добавление атрибута");
                return false;
            }

            return base.AddAttribute(attribute);
        }

        /// <summary>
        /// Удалить атрибут, если рабочее дерево не является системным.
        /// </summary>
        /// <param name="attribute">Атрибут.</param>
        /// <returns>true, если атрибут удален; false, если рабочее дерево системное или операция не выполнена.</returns>
        /// <remarks>Implements requirement R-5.05 for the system working tree.</remarks>
        public override bool RemoveAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (IsSystemBase)
            {
                SendAttributeCollectionRestriction("R-5.05", "удаление атрибута");
                return false;
            }

            return base.RemoveAttribute(attribute);
        }

        /// <summary>
        /// Очистить атрибуты, если рабочее дерево не является системным.
        /// </summary>
        /// <returns>true, если атрибуты очищены; false, если рабочее дерево системное.</returns>
        /// <remarks>Implements requirement R-5.05 for the system working tree.</remarks>
        public override bool ClearAttributes()
        {
            if (IsSystemBase)
            {
                SendAttributeCollectionRestriction("R-5.05", "очистка атрибутов");
                return false;
            }

            return base.ClearAttributes();
        }

        private void SendAttributeCollectionRestriction(string requirementCode, string operation)
        {
            _notificationService.SendTextMessage<WorkingTreeModel>(
                $"{requirementCode}: Для системного рабочего дерева '{Name}' [{Uuid}] операция '{operation}' запрещена. " +
                "Атрибуты элементов системного рабочего дерева не редактируются пользователем.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }

        /// <summary>
        /// Получить все узлы рабочего дерева (рекурсивно).
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
