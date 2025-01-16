using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.RepositoryInterfaces
{
    public interface IInfrastructureRepository
    {
        public InfrastructureRepositoryTypes InftastructureRepositoryTypes { get; }
    }
}
