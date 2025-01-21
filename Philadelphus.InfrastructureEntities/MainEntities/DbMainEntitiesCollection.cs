using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbMainEntitiesCollection
    {
        public IEnumerable<DbTreeRoot> DbTreeRoots { get; set; }
        public IEnumerable<DbTreeNode> DbTreeNodes { get; set; }
        public IEnumerable<DbTreeLeave> DbTreeLeaves { get; set; }
        public IEnumerable<DbEntityAttribute> DbAttributes { get; set; }
        public IEnumerable<DbAttributeEntry> DbAttributeEntries { get; set; }
        public IEnumerable<DbAttributeValue> DbAttributeValues { get; set; }
    }
}
