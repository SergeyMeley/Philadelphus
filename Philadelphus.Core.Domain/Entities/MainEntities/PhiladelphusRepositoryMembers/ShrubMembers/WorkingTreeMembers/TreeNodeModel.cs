using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Узел дерева участников репозитория Чубушника (аналог классов и интерфейсов в ООП)
    /// </summary>
    public class TreeNodeModel : WorkingTreeMemberBaseModel<TreeNodeModel>, IWorkingTreeMemberModel, IParentModel, IChildrenModel, IOwnerModel
    {
        #region [ Fields ]

        private readonly List<TreeNodeModel> _childNodes;
        private readonly List<TreeLeaveModel> _childLeaves;
        private readonly HashSet<Guid> _childNodeUuids = new();
        private readonly HashSet<Guid> _childLeaveUuids = new();
        private TreeLeaveModel? _polymorphicParentLeave;

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый узел";

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Тип.
        /// </summary>
        [Display(Name = "[Системный тип]", Description = "Системный базовый тип")]
        public virtual SystemBaseType SystemBaseType { get => SystemBaseType.USER_DEFINED; }

        #endregion

        #region [ Hierarchy Properties ]

        /// <summary>
        /// Родитель
        /// </summary>
        [Display(Name = "[Родитель]", Description = "Родительский узел")]
        public TreeNodeModel ParentNode { get; }

        /// <summary>
        /// Runtime-only лист родительского узла, соответствующий текущему узлу.
        /// </summary>
        public TreeLeaveModel? PolymorphicParentLeave => _polymorphicParentLeave;

        /// <summary>
        /// Родитель
        /// </summary>
        [Display(Name = "[Родитель]", Description = "Родитель")]
        public IParentModel Parent
        {
            get
            {
                return (IParentModel)ParentNode ?? OwningWorkingTree.ContentRoot;
            }
        }

        /// <summary>
        /// Все родители (рекурсивно)
        /// </summary>
        [Display(Name = "[Родители]", Description = "Все родители (рекурсивно)")]
        public ReadOnlyDictionary<Guid, IParentModel> AllParentsRecursive
        {
            get => RecursiveRelationshipHelper.ToReadOnlyDictionary(
                RecursiveRelationshipHelper.EnumerateParentsRecursive(this));
        }

        /// <summary>
        /// Дочерние узлы
        /// </summary>
        [Display(Name = "[Узлы]", Description = "Дочерние узлы")]
        public IReadOnlyList<TreeNodeModel> ChildNodes { get => _childNodes; }

        /// <summary>
        /// Дочерние листы
        /// </summary>
        [Display(Name = "[Листы]", Description = "Дочерние листы")]
        public IReadOnlyList<TreeLeaveModel> ChildLeaves { get => _childLeaves; }

        /// <summary>
        /// Наследники
        /// </summary>
        [Display(Name = "[Наследники]", Description = "Наследники")]
        public ReadOnlyDictionary<Guid, IChildrenModel> Childs
        {
            get
            {
                var result = new Dictionary<Guid, IChildrenModel>();

                foreach (var node in _childNodes)
                {
                    result.Add(node.Uuid, node);
                }

                foreach (var leave in _childLeaves)
                {
                    result.Add(leave.Uuid, leave);
                }

                return result.AsReadOnly();
            }
        }

        /// <summary>
        /// Все наследники (рекурсивно)
        /// </summary>
        [Display(Name = "[Наследники]", Description = "Все наследники (рекурсивно)")]
        public ReadOnlyDictionary<Guid, IChildrenModel> AllChildsRecursive
        {
            get => RecursiveRelationshipHelper.ToReadOnlyDictionary(
                RecursiveRelationshipHelper.EnumerateChildsRecursive(this));
        }

        #endregion

        #region [ Ownership Properties ]



        #endregion

        #region [ Infrastructure Properties ]



        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Узел репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Родительский узел или корень Чубушника</param>
        internal TreeNodeModel(
            Guid uuid,
            IParentModel parent,
            WorkingTreeModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<TreeNodeModel> propertiesPolicy)
            : base(uuid, owner, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(parent);

            if (parent is TreeNodeModel node)
                ParentNode = node;

            Parent.AddChild(this);
            OwningWorkingTree.ContentNodes.Add(this);

            _childNodes = new List<TreeNodeModel>();
            _childLeaves = new List<TreeLeaveModel>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить родителя
        /// </summary>
        /// <param name="newParent">Новый родительский элемент.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public bool ChangeParent(IParentModel newParent)
        {
            ArgumentNullException.ThrowIfNull(newParent);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Устанавливает вычисленную полиморфную связь с листом прямого родительского узла.
        /// </summary>
        internal bool SetPolymorphicParentLeave(TreeLeaveModel? parentLeave)
        {
            if (_polymorphicParentLeave?.Uuid == parentLeave?.Uuid)
                return false;

            if (parentLeave != null
                && (parentLeave.Uuid == Uuid
                    || ParentNode == null
                    || parentLeave.ParentNode.Uuid != ParentNode.Uuid))
            {
                return false;
            }

            _polymorphicParentLeave = parentLeave;
            return true;
        }

        /// <summary>
        /// Добавить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <remarks>Implements requirement R-5.04 for BOOL system nodes.</remarks>
        public bool AddChild(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            if (child is TreeNodeModel n
                && _childNodeUuids.Add(child.Uuid))
            {
                _childNodes.Add(n);
                return true;
            }
            else if (child is TreeLeaveModel l
                && _childLeaveUuids.Add(child.Uuid))
            {
                if (IsSystemBaseBoolNode())
                {
                    _childLeaveUuids.Remove(child.Uuid);
                    if (IsPredefinedSystemBaseBoolLeave(l) == false
                        && l is not SystemBaseTreeLeaveModel)
                    {
                        SendSystemBaseBoolNodeRestriction("R-5.04", "добавление дочерних листов");
                    }

                    return false;
                }

                _childLeaves.Add(l);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Регистрирует предопределенный системный лист логического типа при инициализации системного справочника.
        /// </summary>
        /// <param name="leave">Предопределенный системный лист BOOL.</param>
        /// <returns>true, если лист добавлен; иначе false.</returns>
        internal bool AddPredefinedSystemBaseBoolLeave(TreeLeaveModel leave)
        {
            ArgumentNullException.ThrowIfNull(leave);

            if (IsSystemBaseBoolNode() == false
                || IsPredefinedSystemBaseBoolLeave(leave) == false
                || _childLeaveUuids.Add(leave.Uuid) == false)
            {
                return false;
            }

            _childLeaves.Add(leave);
            return true;
        }

        /// <summary>
        /// Удалить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <remarks>Implements requirement R-5.04 for BOOL system nodes.</remarks>
        public bool RemoveChild(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            if (child is TreeNodeModel n
                && _childNodeUuids.Remove(child.Uuid))
            {
                var remItem = _childNodes.First(x => x.Uuid == child.Uuid);
                _childNodes.Remove(remItem);
                return true;
            }
            else if (child is TreeLeaveModel l
                && _childLeaveUuids.Remove(child.Uuid))
            {
                if (IsSystemBaseBoolNode())
                {
                    _childLeaveUuids.Add(child.Uuid);
                    SendSystemBaseBoolNodeRestriction("R-5.04", "удаление дочерних листов");
                    return false;
                }

                var remItem = _childLeaves.First(x => x.Uuid == child.Uuid);
                _childLeaves.Remove(remItem);
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Очистить список наследников
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <remarks>Implements requirement R-5.04 for BOOL system nodes.</remarks>
        public bool ClearChilds()
        {
            if (IsSystemBaseBoolNode()
                && _childLeaves.Count != 0)
            {
                SendSystemBaseBoolNodeRestriction("R-5.04", "очистка дочерних листов");
                return false;
            }

            _childNodes.Clear();
            _childLeaves.Clear();
            _childNodeUuids.Clear();
            _childLeaveUuids.Clear();
            return true;
        }

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        protected override bool AddContentDetailed(IContentModel content)
        {
            return true;
        }

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        protected override bool RemoveContentDetailed(IContentModel content)
        {
            return true;
        }

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        protected override bool ClearContentDetailed()
        {
            return true;
        }

        /// <summary>
        /// Проверить, является ли узел системным булевым типом с фиксированным набором листьев.
        /// </summary>
        /// <returns>true, если узел представляет системный тип bool; иначе false.</returns>
        private bool IsSystemBaseBoolNode()
        {
            return this is SystemBaseTreeNodeModel { SystemBaseType: SystemBaseType.BOOL };
        }

        private void SendSystemBaseBoolNodeRestriction(string requirementCode, string operation)
        {
            _notificationService.SendTextMessage<TreeNodeModel>(
                $"{requirementCode}: Для системного узла логического типа '{Name}' [{Uuid}] операция '{operation}' запрещена. " +
                "Набор дочерних листов BOOL является фиксированным системным справочником.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }

        /// <summary>
        /// Проверить, является ли лист предопределенным системным значением bool.
        /// </summary>
        /// <param name="leave">Проверяемый лист.</param>
        /// <returns>true, если лист входит в фиксированный набор системных bool-значений; иначе false.</returns>
        private static bool IsPredefinedSystemBaseBoolLeave(TreeLeaveModel leave)
        {
            return leave is SystemBaseTreeLeaveModel
                && SystemBaseTreeLeaveModel.IsSystemBaseValue(leave.Uuid)
                && SystemBaseTreeLeaveModel.GetTypeByUuid(leave.Uuid) == SystemBaseType.BOOL;
        }

        #endregion
    }
}
