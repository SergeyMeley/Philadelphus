using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers
{

    public class TreeRoot : MainEntityBase
    {
        public Guid OwnDataStorageUuid { get; set; }
        public virtual TreeNode[] ChildTreeNodes { get; set; }
        public TreeRoot()
        {
            
        }
    }
}
