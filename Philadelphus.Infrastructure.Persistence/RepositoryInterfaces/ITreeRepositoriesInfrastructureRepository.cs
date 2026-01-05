using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface ITreeRepositoriesInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<TreeRepository> SelectRepositories();
        public IEnumerable<TreeRepository> SelectRepositories(Guid[] uuids);
        public long InsertRepository(TreeRepository item);
        public long DeleteRepository(TreeRepository item);
        public long UpdateRepository(TreeRepository item);
    }
}
