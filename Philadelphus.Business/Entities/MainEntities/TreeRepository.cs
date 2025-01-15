using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRepository : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Repository; }
        public string DirectoryPath { get; set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public IEnumerable<TreeRoot> ChildTreeRoots { get; set; } = new List<TreeRoot>();
        public TreeRepository(string name, Guid parentGuid) : base(name, parentGuid)
        {
            
        }
    }
}
