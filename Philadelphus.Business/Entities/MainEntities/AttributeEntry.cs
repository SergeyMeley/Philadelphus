using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class AttributeEntry : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public EntityAttribute Attribute { get; set; }
        public AttributeValue AttributeValue { get; set; }
        public long EntityId { get; set; }
        public AttributeEntry(Guid parentGuid) : base(parentGuid)
        {

        }
        public AttributeEntry(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {

        }
    }
}
