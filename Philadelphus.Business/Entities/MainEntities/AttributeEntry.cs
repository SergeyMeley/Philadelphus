using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class AttributeEntry : MainEntityBase
    {
        public override EntityTypes entityType { get => EntityTypes.None; }
        public Attribute Attribute { get; set; }
        public AttributeValue AttributeValue { get; set; }
        public EntityTypes EntityType { get; set; }
        public long EntityId { get; set; }
    }
}
