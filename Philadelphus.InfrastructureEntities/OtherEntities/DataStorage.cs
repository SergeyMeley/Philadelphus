using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.OtherEntities
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
    }
}
