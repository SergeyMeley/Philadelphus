using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbTreeLeave : DbEntityBase
    {
        public long ParentTreeNodeId { get; set; }
        public string Uuid { get; set; }
        public string Type { get; set; }
        public List<long> AttributeIds { get; set; } = new List<long>();
        public DbTreeLeave()
        {

        }
    }
}
