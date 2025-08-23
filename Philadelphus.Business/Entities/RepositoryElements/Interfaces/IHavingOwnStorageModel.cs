using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    /// <summary>
    /// Реализует возможность наличия независимой инфраструктуры (места и способа хранения данных)
    /// </summary>
    internal interface IHavingOwnStorageModel
    {
        public IMainEntitiesInfrastructure Infrastructure { get; }
    }
}
