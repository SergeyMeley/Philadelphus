using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbNode : DbEntityBase
    {
        public IEnumerable<long> AttributeIds { get; set; }
        public IEnumerable<long> ChildNodeIds { get; set; }
        public IEnumerable<long> ElementIds { get; set; }
        public long ProjectId { get; set; }
        public long LayerId { get; set; }
        public long ParentNodeId { get; set; }
        public DbNode(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
