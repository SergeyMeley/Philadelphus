using Philadelphus.Infrastructure.Persistence.MainEntities;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Interfaces
{
    public interface ITreeRepositoryHeadersCollectionInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<TreeRepositoryHeader> SelectRepositoryCollection();
        public long UpdateRepository(TreeRepositoryHeader treeRepositoryHeader);
    }
}
