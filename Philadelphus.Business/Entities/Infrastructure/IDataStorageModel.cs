using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Infrastructure
{
    public interface IDataStorageModel
    {
        public Guid Guid { get; }
        public string Name { get; }
        public string Description { get; }
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
