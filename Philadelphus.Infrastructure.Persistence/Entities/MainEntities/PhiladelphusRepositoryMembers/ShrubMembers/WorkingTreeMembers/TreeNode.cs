namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    public class TreeNode : WorkingTreeMemberBase
    {
        public Guid? ParentTreeRootUuid { get; set; }
        public Guid? ParentTreeNodeUuid { get; set; }
        public int SystemBaseTypeId { get; set; }
        public virtual ICollection<TreeNode> ChildNodes { get; set; } = new List<TreeNode>();
        public virtual ICollection<TreeLeave> ChildLeaves { get; set; } = new List<TreeLeave>();

        public TreeNode()
        {

        }
    }
}
