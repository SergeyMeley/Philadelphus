using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class MainEntitiesCollection
    {
        public IEnumerable<TreeRoot> DbTreeRoots { get; set; }
        public IEnumerable<TreeNode> DbTreeNodes { get; set; }
        public IEnumerable<TreeLeave> DbTreeLeaves { get; set; }
        public IEnumerable<ElementAttribute> DbAttributes { get; set; }
        public IEnumerable<AttributeEntry> DbAttributeEntries { get; set; }
        public IEnumerable<AttributeValue> DbAttributeValues { get; set; }
    }
}
