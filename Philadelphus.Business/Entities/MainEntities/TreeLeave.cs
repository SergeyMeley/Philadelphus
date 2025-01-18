using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeLeave : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Leave; }
        public override InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public string Uuid { get; set; }
        public string Type { get; set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public TreeLeave(string name, Guid parentGuid) : base(name, parentGuid)
        {
            
        }
    }
}
