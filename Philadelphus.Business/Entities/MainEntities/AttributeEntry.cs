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
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public Attribute Attribute { get; set; }
        public AttributeValue AttributeValue { get; set; } = new AttributeValue();
        public long EntityId { get; set; }
        public AttributeEntry(string name, long id) : base(name, id)
        {

        }
        public AttributeEntry(string name, string path) : base(name, path)
        {

        }
    }
}
