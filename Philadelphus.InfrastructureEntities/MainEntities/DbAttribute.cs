using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbAttribute : DbEntityBase
    {
        public string ValueType { get; set; }
        public IEnumerable<double> ValueIds { get; set; }
        public DbAttribute(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
