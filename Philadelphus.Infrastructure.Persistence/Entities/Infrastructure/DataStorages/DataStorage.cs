using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages
{
    public class DataStorage
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public InfrastructureTypes InfrastructureType { get; set; }
        public bool HasPhiladelphusRepositoriesInfrastructureRepository { get; set; }
        public bool HasShrubMembersInfrastructureRepository { get; set; }
        public bool IsHidden { get; set; }
    }
}
