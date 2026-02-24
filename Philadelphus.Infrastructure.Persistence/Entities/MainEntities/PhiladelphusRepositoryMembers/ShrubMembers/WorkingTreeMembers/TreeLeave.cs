namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    public class TreeLeave : WorkingTreeMemberBase
    {
        public Guid? ParentTreeNodeUuid { get; set; }
        public virtual TreeNode ParentTreeNode { get; set; }
        public int SystemBaseTypeId { get; set; }
        public TreeLeave()
        {

        }
    }
}
