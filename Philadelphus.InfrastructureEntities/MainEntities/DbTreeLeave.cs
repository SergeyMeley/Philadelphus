using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbTreeLeave : DbEntityBase
    {
        public string Uuid { get; set; }
        public string Type { get; set; }
        public IEnumerable<long> AttributeIds { get; set; }
        public long ParentTreeNodeId { get; set; }
        public DbTreeLeave(long id, string name) : base(id, name)
        {
            AttributeIds = new List<long>();
        }
    }
}
