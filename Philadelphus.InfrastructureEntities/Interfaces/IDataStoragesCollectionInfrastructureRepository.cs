using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.Interfaces
{
    public interface IDataStoragesCollectionInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<DataStorage> SelectDataStorages();
        public long UpdateDataStorages(IEnumerable<DataStorage> storages);
    }
}
