using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbMainEntitiesCollection
    {
        public IEnumerable<DbTreeRepository> TreeRepositories { get; set; }
        public IEnumerable<DbTreeRoot> TreeRoots { get; set; }
        public IEnumerable<DbTreeNode> TreeNodes { get; set; }

    }
}
