namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{

    public class TreeRoot : WorkingTreeMemberBase
    {
        public virtual ICollection<TreeNode> ChildNodes { get; set; } = new List<TreeNode>();

        /// <summary>
        /// Корень рабочего дерева
        /// </summary>
        public TreeRoot()
        {
            
        }
    }
}
