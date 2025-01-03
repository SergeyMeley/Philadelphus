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
        public IEnumerable<long> AttributeIds { get; set; }
        public IEnumerable<long> ChildTreeNodeIds { get; set; }
        public IEnumerable<long> ChildTreeLeaveIds { get; set; }
        public DbTreeNode(long id, string name) : base(id, name)
        {
            AttributeIds = new List<long>();
            ChildTreeNodeIds = new List<long>();
            ChildTreeLeaveIds = new List<long>();
        }
    }
}
