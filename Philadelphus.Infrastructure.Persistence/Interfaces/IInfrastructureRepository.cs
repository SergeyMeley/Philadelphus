using Philadelphus.Infrastructure.Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Interfaces
{
    public interface IInfrastructureRepository
    {
        //public InfrastructureTypes InfrastructureRepositoryTypes { get; }
        public InfrastructureEntityGroups EntityGroup { get; }
        public bool CheckAvailability();
    }
}
