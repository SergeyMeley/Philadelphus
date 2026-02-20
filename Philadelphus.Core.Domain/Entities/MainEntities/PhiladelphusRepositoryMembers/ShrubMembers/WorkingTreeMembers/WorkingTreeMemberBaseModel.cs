using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Участник корня репозитория Чубушника
    /// </summary>
    public abstract class WorkingTreeMemberBaseModel : ShrubMemberBaseModel, IWorkingTreeMemberModel, IOwnerModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый участник дерева";
        
        private string _customCode;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Пользовательский код
        /// Уникален в рамках дерева сущностей
        /// </summary>
        public string CustomCode
        {
            get
            {
                return _customCode;
            }
            set
            {
                if (_customCode != value)
                {
                    _customCode = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Родительский корень репозитория Чубушника
        /// </summary>
        public WorkingTreeModel OwningWorkingTree { get; }

        /// <summary>
        /// Содержимое
        /// </summary>
        public override ReadOnlyDictionary<Guid, IContentModel> Content { get; }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage
        { 
            get => OwningWorkingTree.DataStorage;
        }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Участник корня репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Родительский элемент Чубушника</param>
        /// <param name="dbEntity">Сущность БД</param>
        public WorkingTreeMemberBaseModel(
            Guid uuid,
            WorkingTreeModel owner,
            IMainEntity dbEntity)
             : base(uuid, owner.OwningShrub, dbEntity)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            OwningWorkingTree = owner;
        }

        #endregion

        #region [ Methods ]

        internal IEnumerable<TreeNodeModel> GetAllNodesRecursive()
        {
            var nodes = new List<TreeNodeModel>();

            if (this is TreeRootModel root)
            {
                nodes.AddRange(root.ChildNodes);

                foreach (var nodeItem in root.ChildNodes)
                {
                    nodes.AddRange(nodeItem.GetAllNodesRecursive());
                }
            }

            if (this is TreeNodeModel node)
            {
                nodes.AddRange(node.ChildNodes);

                foreach (var nodeItem in node.ChildNodes)
                {
                    nodes.AddRange(nodeItem.GetAllNodesRecursive());
                }
            }

            return nodes;
        }

        internal IEnumerable<TreeLeaveModel> GetAllLeavesRecursive()
        {
            var leaves = new List<TreeLeaveModel>();

            if (this is TreeRootModel root)
            {
                foreach (var nodeItem in root.ChildNodes)
                {
                    leaves.AddRange(nodeItem.GetAllLeavesRecursive());
                }
            }

            if (this is TreeNodeModel node)
            {
                leaves.AddRange(node.ChildLeaves);

                foreach (var leaveItem in node.ChildLeaves)
                {
                    leaves.AddRange(leaveItem.GetAllLeavesRecursive());
                }
            }

            return leaves;
        }

        #endregion
    }
}
