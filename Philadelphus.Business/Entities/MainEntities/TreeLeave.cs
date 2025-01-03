using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeLeave : MainEntityBase
    {
        public override EntityTypes entityType { get => EntityTypes.Leave; }
        public string Uuid { get; set; }
        public string Type { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }
        public TreeLeave(List<Attribute> attributes)
        {
            Attributes = attributes;
        }
    }
}
