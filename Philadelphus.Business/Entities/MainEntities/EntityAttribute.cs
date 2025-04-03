using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class EntityAttribute : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public ValueTypes ValueType { get; set; }
        public IEnumerable<AttributeValue> Values { get; set; } = new List<AttributeValue>();
        public EntityAttribute(Guid parentGuid) : base(parentGuid)
        {

        }
        public EntityAttribute(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {

        }
    }
}
