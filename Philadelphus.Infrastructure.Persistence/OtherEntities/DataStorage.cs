using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.OtherEntities
{
    public class DataStorage
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public InfrastructureTypes InfrastructureType { get; set; }
        public bool HasDataStorageInfrastructureRepositoryRepository { get; set; }
        public bool HasTreeRepositoryHeadersInfrastructureRepository { get; set; }
        public bool HasMainEntitiesInfrastructureRepository { get; set; }
        public bool IsDisabled { get; set; }
    }
}
