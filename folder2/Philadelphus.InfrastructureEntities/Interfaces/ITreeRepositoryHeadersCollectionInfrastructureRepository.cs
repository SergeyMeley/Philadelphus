using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.Interfaces
{
    public interface ITreeRepositoryHeadersCollectionInfrastructureRepository : IInfrastructureRepository
    {
        public IEnumerable<TreeRepositoryHeader> SelectRepositoryCollection();
        public long UpdateRepository(TreeRepositoryHeader treeRepositoryHeader);
    }
}
