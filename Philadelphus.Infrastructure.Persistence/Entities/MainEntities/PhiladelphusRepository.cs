using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    public class PhiladelphusRepository : MainEntityBase
    {
        public Guid OwnDataStorageUuid { get; set; }
        public Guid[] ContentWorkingTreesUuids { get; set; }
        public PhiladelphusRepository()
        {
            
        }
    }
}
