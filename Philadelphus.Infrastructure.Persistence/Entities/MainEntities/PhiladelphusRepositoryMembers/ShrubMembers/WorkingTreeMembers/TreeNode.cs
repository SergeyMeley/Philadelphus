namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    public class TreeNode : WorkingTreeMemberBase
    {
        public Guid? ParentTreeRootUuid { get; set; }
        public virtual TreeRoot ParentTreeRoot { get; set; }
        public Guid? ParentTreeNodeUuid { get; set; }
        public virtual TreeNode ParentTreeNode { get; set; }
        public int SystemBaseTypeId { get; set; }
        public TreeNode()
        {

        }
    }
}
