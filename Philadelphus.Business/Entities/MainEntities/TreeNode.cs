using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeNode : MainEntityBase
    {
        public IEnumerable<long> Attributes { get; set; }
        public IEnumerable<long> ChildTreeNodes { get; set; }
        public IEnumerable<long> ChildTreeLeaves { get; set; }
        public TreeNode()
        {
            Attributes = new List<long>();
            ChildTreeNodes = new List<long>();
            ChildTreeLeaves = new List<long>();
        }
    }
}
