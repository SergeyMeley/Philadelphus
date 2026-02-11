namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers
{
    public abstract class TreeRootMemberBase : MainEntityBase
    {
        public Guid? ParentTreeRootUuid { get; set; }
        public virtual TreeRoot ParentTreeRoot { get; set; }
        public Guid? ParentUuid { get; set; }
        public virtual IMainEntity Parent
        {
            get
            {
                return ParentTreeRoot;
            }
            set
            {
                ParentTreeRoot = (TreeRoot)value;
            }
        }
    }
}
