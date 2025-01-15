using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class Attribute : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Attribute; }
        public ValueTypes ValueType { get; set; }
        public IEnumerable<AttributeValue> Values { get; set; } = new List<AttributeValue>();
        public Attribute(string name, Guid parentGuid) : base(name, parentGuid)
        {

        }
    }
}
