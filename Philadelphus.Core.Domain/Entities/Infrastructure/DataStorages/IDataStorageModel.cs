using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;

namespace Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages
{
    public interface IDataStorageModel
    {
        public Guid Uuid { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public InfrastructureTypes InfrastructureType { get; }
        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories { get; }
        public IDataStoragesCollectionInfrastructureRepository DataStoragesCollectionInfrastructureRepository { get; }
        public ITreeRepositoryHeadersCollectionInfrastructureRepository TreeRepositoryHeadersCollectionInfrastructureRepository { get; }
        public ITreeRepositoriesInfrastructureRepository TreeRepositoriesInfrastructureRepository { get; }
        public IMainEntitiesInfrastructureRepository MainEntitiesInfrastructureRepository { get; }
        public bool IsAvailable { get; }
        public bool IsDisabled { get; set; }
        public DateTime LastCheckTime { get; }
        public bool CheckAvailable();
        public bool StartAvailableAutoChecking();
    }
}
