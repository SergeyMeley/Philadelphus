using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface IPhiladelphusRepositoriesInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<PhiladelphusRepository> SelectRepositories();
        public IEnumerable<PhiladelphusRepository> SelectRepositories(Guid[] uuids);
        public long InsertRepository(PhiladelphusRepository item);
        public long DeleteRepository(PhiladelphusRepository item);
        public long UpdateRepository(PhiladelphusRepository item);
    }
}
