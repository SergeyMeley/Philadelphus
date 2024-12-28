using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbMainEntitiesCollection
    {
        public IEnumerable<DbLayer> Layers { get; set; }
        public IEnumerable<DbNode> Nodes { get; set; }
    }
}
