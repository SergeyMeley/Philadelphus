using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class EntityAttribute : EntityBase
    {
        public string ValueType { get; set; }
        public List<long> ValueIds { get; set; } = new List<long>();
        public EntityAttribute()
        {
            
        }
    }
}
