using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbTreeLeave : DbEntityBase
    {
        public Guid Guid { get; set; }
        public long ParentTreeNodeGuid { get; set; }
        public string Type { get; set; }
        public List<long> AttributeGuids { get; set; } = new List<long>();
        public DbTreeLeave()
        {

        }
    }
}
