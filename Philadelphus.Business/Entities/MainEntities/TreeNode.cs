using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
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
        public override InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public IEnumerable<TreeNode> ChildTreeNodes { get; set; } = new List<TreeNode>();
        public IEnumerable<TreeLeave> ChildTreeLeaves { get; set; } = new List<TreeLeave>();
        public TreeNode(string name, Guid parentGuid) : base(name, parentGuid)
        {
            
        }
    }
}
