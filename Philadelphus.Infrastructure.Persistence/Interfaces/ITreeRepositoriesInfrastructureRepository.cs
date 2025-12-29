using Philadelphus.Infrastructure.Persistence.MainEntities;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Interfaces
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
