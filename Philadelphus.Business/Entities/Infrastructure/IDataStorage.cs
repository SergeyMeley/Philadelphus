using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Infrastructure
{
    public interface IDataStorage
    {
        public Guid Guid { get; }
        public string Name { get; }
        public string Description { get; }
        InfrastructureTypes InfrastructureType { get; }
        public IInfrastructureRepository InfrastructureRepository { get; }
    }
}
