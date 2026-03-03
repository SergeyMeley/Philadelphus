using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages
{
    public class DataStorage
    {
        public Guid Uuid { get; set; }
        public string ProviderName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public InfrastructureTypes InfrastructureType { get; set; }
        public bool HasDataStorageInfrastructureRepositoryRepository { get; set; }
        public bool HasPhiladelphusRepositoryHeadersInfrastructureRepository { get; set; }
        public bool HasMainEntitiesInfrastructureRepository { get; set; }
        public bool IsDisabled { get; set; }
    }
}
