using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    internal interface IIndependentInfrastructure
    {
        public IMainEntitiesRepository Infrastructure { get; }
    }
}
