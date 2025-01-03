using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class Attribute : MainEntityBase
    {
        public override EntityTypes entityType { get => EntityTypes.Attribute; }
        public ValueTypes ValueType { get; set; }
        public IEnumerable<AttributeValue> Values { get; set; }
        public Attribute()
        {
        }
    }
}
