using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
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
        public IMainEntitiesRepository Infrastructure { get; private set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public IEnumerable<TreeNode> ChildTreeNodes { get; set; } = new List<TreeNode>();
        public IEnumerable<TreeLeave> ChildTreeLeaves { get; set; } = new List<TreeLeave>();
        public TreeNode(Guid parentGuid) : base(parentGuid)
        {
            Childs = new List<IMainEntity>();
        }
        public TreeNode(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {
            Childs = new List<IMainEntity>();
        }
    }
}
