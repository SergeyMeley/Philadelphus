using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers
{

    public class TreeRoot : MainEntityBase
    {
        public Guid OwnDataStorageUuid { get; set; }
        public Guid[] DataStoragesUuids { get; set; }   //TODO: Удалить
        public virtual TreeNode[] ChildTreeNodes { get; set; }
        public TreeRoot()
        {
            
        }
    }
}
