using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbTreeNode : DbEntityBase
    {
        public long ParentTreeRootId { get; set; }
        public long ParentTreeNodeId { get; set; }
        public List<long> AttributeIds { get; set; } = new List<long>();
        public List<long> ChildTreeNodeIds { get; set; } = new List<long>();
        public List<long> ChildTreeLeaveIds { get; set; } = new List<long>();
        public DbTreeNode()
        {

        }
    }
}
