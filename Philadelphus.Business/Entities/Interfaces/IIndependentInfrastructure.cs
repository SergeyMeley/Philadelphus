using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Interfaces
{
    /// <summary>
    /// Реализует возможность наличия независимой инфраструктуры (места и способа хранения данных)
    /// </summary>
    internal interface IIndependentInfrastructure
    {
        public IMainEntitiesRepository Infrastructure { get; }
    }
}
