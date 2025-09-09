using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.JsonRepository.Repositories
{
    public class JsonDataStorageAndTreeRepositoryInfrastructureRepository : IDataStorageInfrastructureRepository, ITreeRepositoryHeadersInfrastructureRepository
    {
        public bool CheckAvailability()
        {
            return true;
        }

        public IEnumerable<DataStorage> SelectDataStorages()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRepository> SelectRepositories()
        {
            throw new NotImplementedException();
        }

        public long UpdateDataStorages(IEnumerable<DataStorage> storages)
        {
            throw new NotImplementedException();
        }

        public long UpdateRepositories(IEnumerable<TreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long DeleteDataStorages(IEnumerable<DataStorage> storages)
        {
            throw new NotImplementedException();
        }

        public long DeleteRepositories(IEnumerable<TreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long InsertDataStorages(IEnumerable<DataStorage> storages)
        {
            throw new NotImplementedException();
        }

        public long InsertRepositories(IEnumerable<TreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

    }
}
