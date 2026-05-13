namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Представляет объект узла рабочего дерева.
    /// </summary>
    public class TreeNode : WorkingTreeMemberBase
    {
        /// <summary>
        /// Корень рабочего дерева.
        /// </summary>
        public Guid? ParentTreeRootUuid { get; set; }
        
        /// <summary>
        /// Родительский узел рабочего дерева.
        /// </summary>
        public Guid? ParentTreeNodeUuid { get; set; }
        
        /// <summary>
        /// Тип.
        /// </summary>
        public int SystemBaseTypeId { get; set; }
       
        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        public virtual ICollection<TreeNode> ChildNodes { get; set; } = new List<TreeNode>();
       
        /// <summary>
        /// Дочерние листы.
        /// </summary>
        public virtual ICollection<TreeLeave> ChildLeaves { get; set; } = new List<TreeLeave>();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeNode" />.
        /// </summary>
        public TreeNode()
        {

        }
    }
}
