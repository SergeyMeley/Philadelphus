using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface IDataStoragesCollectionInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<DataStorage> SelectDataStorages();
        public long UpdateDataStorages(IEnumerable<DataStorage> storages);
    }
}
