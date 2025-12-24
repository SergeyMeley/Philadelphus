using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class ElementAttribute : MainEntityBase
    {
        public Guid OwnerUuid { get; set; }
        public Guid? ValueTypeUuid { get; set; }
        public Guid? ValueUuid { get; set; }
        public ElementAttribute()
        {
            
        }
    }
}
