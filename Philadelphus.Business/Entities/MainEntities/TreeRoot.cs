using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRoot : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Root; }
        public override InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public string DirectoryPath { get; set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public IEnumerable<TreeNode> ChildTreeNodes { get; set; } = new List<TreeNode>();
        public TreeRoot(string name, Guid parentGuid) : base(name, parentGuid)
        {

        }
    }
}
