using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Узел дерева участников репозитория Чубушника (аналог классов и интерфейсов в ООП)
    /// </summary>
    public class TreeNodeModel : WorkingTreeMemberBaseModel, IWorkingTreeMemberModel, IParentModel, IChildrenModel, IOwnerModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый узел";

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]



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
        public List<TreeNodeModel> ChildNodes { get; }

        /// <summary>
        /// Дочерние листы
        /// </summary>
        public List<TreeLeaveModel> ChildLeaves { get; }

        /// <summary>
        /// Наследники
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> Childs
        {
            get
            {
                var result = new Dictionary<Guid, IChildrenModel>();

                foreach (var node in ChildNodes)
                {
                    result.Add(node.Uuid, node);
                }

                foreach (var leave in ChildLeaves)
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
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeNodeModel(
            Guid uuid,
            IParentModel parent,
            WorkingTreeModel owner,
            IMainEntity dbEntity)
            : base(uuid, owner, dbEntity)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (parent is TreeNodeModel node)
                ParentNode = node;

            ChildNodes = new List<TreeNodeModel>();
            ChildLeaves = new List<TreeLeaveModel>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить родителя
        /// </summary>
        public bool ChangeParent(IParentModel newParent)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
