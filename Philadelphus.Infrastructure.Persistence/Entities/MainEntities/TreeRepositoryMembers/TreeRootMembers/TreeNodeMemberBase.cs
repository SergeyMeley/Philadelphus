namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers
{
    public class TreeNodeMemberBase : TreeRootMemberBase
    {
        public Guid? ParentTreeNodeUuid { get; set; }
        public virtual TreeNode ParentTreeNode { get; set; }
        public virtual IMainEntity Parent
        {
            get
            {
                if (ParentTreeNode != null)
                {
                    return ParentTreeNode;
                }
                else
                {
                    return ParentTreeRoot;
                }
            }
            set
            {
                if (value.GetType() == typeof(TreeRoot))
                {
                    ParentTreeRoot = (TreeRoot)value;
                }
                else if (value.GetType() == typeof(TreeNode))
                {
                    ParentTreeRoot = ((TreeNode)value).ParentTreeRoot;
                    ParentTreeNode = (TreeNode)value;
                }
            }
        }
    }
}
