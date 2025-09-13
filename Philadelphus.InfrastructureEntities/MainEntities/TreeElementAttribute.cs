using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class TreeElementAttribute : MainEntityBase
    {
        public long AttributeGuid { get; set; }
        public long AttributeValueGuid { get; set; }
        public long EntityTypeGuid { get; set; }
        public long EntityGuid { get; set; }
        public TreeElementAttribute()
        {
        }
    }
}
