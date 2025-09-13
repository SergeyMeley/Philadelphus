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
        public InfrastructureTypes InfrastructureType { get; set; }
        public IDataStoragesInfrastructureRepository DataStorageInfrastructureRepositoryRepository { get; set; }
        public ITreeRepositoriesInfrastructureRepository TreeRepositoryHeadersInfrastructureRepository { get; set; }
        public IMainEntitiesInfrastructureRepository MainEntitiesInfrastructureRepository { get; set; }
        public bool IsAvailable { get; }
    }
}
