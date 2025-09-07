using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.Interfaces
{
    public interface ITreeRepositoryHeadersInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<TreeRepository> SelectRepositories(List<string> pathes);
        public long InsertRepositories(IEnumerable<TreeRepository> repositories);
        public long DeleteRepositories(IEnumerable<TreeRepository> repositories);
        public long UpdateRepositories(IEnumerable<TreeRepository> repositories);

    }
}
