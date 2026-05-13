using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;
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
        public virtual SystemBaseType SystemBaseType { get => SystemBaseType.USER_DEFINED; }

        #endregion

        #region [ Hierarchy Properties ]

        /// <summary>
        /// Родитель
        /// </summary>
        public TreeNodeModel ParentNode { get; }

        /// <summary>
        /// Родитель
        /// </summary>
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
        public ReadOnlyDictionary<Guid, IOwnerModel> AllParentsRecursive
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// Дочерние узлы
        /// </summary>
        public IReadOnlyList<TreeNodeModel> ChildNodes { get => _childNodes; }

        /// <summary>
        /// Дочерние листы
        /// </summary>
        public IReadOnlyList<TreeLeaveModel> ChildLeaves { get => _childLeaves; }

        /// <summary>
        /// Наследники
        /// </summary>
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

        #endregion
    }
}
