using Philadelphus.Infrastructure.Persistence.MainEntities;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Interfaces
{
    public interface IDataStoragesCollectionInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<DataStorage> SelectDataStorages();
        public long UpdateDataStorages(IEnumerable<DataStorage> storages);
    }
}
