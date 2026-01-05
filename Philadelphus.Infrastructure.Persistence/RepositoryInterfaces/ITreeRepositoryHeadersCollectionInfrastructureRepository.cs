using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface ITreeRepositoryHeadersCollectionInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<TreeRepositoryHeader> SelectRepositoryCollection();
        public long UpdateRepository(TreeRepositoryHeader treeRepositoryHeader);
    }
}
