using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface IPhiladelphusRepositoriesInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<PhiladelphusRepository> SelectRepositories(Guid[] uuids = null);
        public long InsertRepository(PhiladelphusRepository item);
        public long SoftDeleteRepository(PhiladelphusRepository item);
        public long UpdateRepository(PhiladelphusRepository item);
    }
}
