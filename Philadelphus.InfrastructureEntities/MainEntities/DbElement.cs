using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbElement : DbEntityBase
    {
        public string Uuid { get; set; }
        public string Type { get; set; }
        public IEnumerable<long> AttributeIds { get; set; }
        public long ProjectId { get; set; }
        public long LayerId { get; set; }
        public long NodeId { get; set; }
        public DbElement(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
