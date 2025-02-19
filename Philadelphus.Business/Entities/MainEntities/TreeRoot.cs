using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRoot : MainEntityBase, IIndependentInfrastructure
    {
        public override EntityTypes EntityType { get => EntityTypes.Root; }
        public override InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public IMainEntitiesRepository Infrastructure { get; private set; }
        public string DirectoryPath { get; set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public IEnumerable<TreeNode> ChildTreeNodes { get; set; } = new List<TreeNode>();
        public TreeRoot(Guid parentGuid) : base(parentGuid)
        {
            Childs = new List<TreeNode>();
        }
        public TreeRoot(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {
            Childs = new List<TreeNode>();
        }
    }
}
