using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.Interfaces
{
    public interface ITreeRepositoriesInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<TreeRepository> SelectRepositories();
        public long InsertRepository(TreeRepository item);
        public long DeleteRepository(TreeRepository item);
        public long UpdateRepository(TreeRepository item);
    }
}
