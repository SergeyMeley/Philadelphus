using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbAttributeEntry : DbEntityBase
    {
        public long AttributeId { get; set; }
        public long AttributeValueId { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public DbAttributeEntry(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
