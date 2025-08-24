using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class TreeNode : EntityBase
    {
        public long ParentTreeRootGuid { get; set; }
        public long ParentTreeNodeGuid { get; set; }
        public List<long> AttributeGuids { get; set; } = new List<long>();
        public List<long> ChildTreeNodeGuids { get; set; } = new List<long>();
        public List<long> ChildTreeLeaveGuids { get; set; } = new List<long>();
        public TreeNode()
        {

        }
    }
}
