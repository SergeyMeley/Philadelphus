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
        [Display(Name = "Системный тип", Description = "Системный базовый тип")]
        public virtual SystemBaseType SystemBaseType { get => SystemBaseType.USER_DEFINED; }

        #endregion

        #region [ Hierarchy Properties ]

        /// <summary>
        /// Родитель
        /// </summary>
        [Display(Name = "Родитель", Description = "Родительский узел")]
        public TreeNodeModel ParentNode { get; }

        /// <summary>
        /// Родитель
        /// </summary>
        [Display(Name = "Родитель", Description = "Родитель")]
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
        [Display(Name = "Родители", Description = "Все родители (рекурсивно)")]
        public ReadOnlyDictionary<Guid, IOwnerModel> AllParentsRecursive
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// Дочерние узлы
        /// </summary>
        [Display(Name = "Узлы", Description = "Дочерние узлы")]
        public IReadOnlyList<TreeNodeModel> ChildNodes { get => _childNodes; }

        /// <summary>
        /// Дочерние листы
        /// </summary>
        [Display(Name = "Листы", Description = "Дочерние листы")]
        public IReadOnlyList<TreeLeaveModel> ChildLeaves { get => _childLeaves; }

        /// <summary>
        /// Наследники
        /// </summary>
        [Display(Name = "Наследники", Description = "Наследники")]
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
        [Display(Name = "Наследники", Description = "Все наследники (рекурсивно)")]
        public ReadOnlyDictionary<Guid, IChildrenModel> AllChildsRecursive
        {
            get => throw new NotImplementedException();
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
        /// Добавить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
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
                if (IsSystemBaseBoolNode()
                    && IsPredefinedSystemBaseBoolLeave(l) == false)
                {
                    _childLeaveUuids.Remove(child.Uuid);
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
        /// Удалить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
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
        public bool ClearChilds()
        {
            if (IsSystemBaseBoolNode()
                && _childLeaves.Count != 0)
            {
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
