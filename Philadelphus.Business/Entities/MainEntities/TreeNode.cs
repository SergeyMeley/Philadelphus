using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeNode : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Node; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public IEnumerable<TreeNode> ChildTreeNodes { get; set; } = new List<TreeNode>();
        public IEnumerable<TreeLeave> ChildTreeLeaves { get; set; } = new List<TreeLeave>();
        public TreeNode(string name, long id) : base(name, id)
        {
            
        }
        public TreeNode(string name, string path) : base(name, path)
        {
            
        }
    }
}
