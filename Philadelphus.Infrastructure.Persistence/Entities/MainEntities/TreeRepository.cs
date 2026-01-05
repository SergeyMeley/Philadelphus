using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    public class TreeRepository
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public virtual AuditInfo AuditInfo { get; set; } = new AuditInfo();
        public Guid OwnDataStorageUuid { get; set; }
        public Guid[] DataStoragesUuids { get; set; }
        public Guid[] ChildTreeRootsUuids { get; set; }
        public TreeRepository()
        {
            
        }
    }
}
