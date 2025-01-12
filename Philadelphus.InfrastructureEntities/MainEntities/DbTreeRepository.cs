using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbTreeRepository : DbEntityBase
    {
        public List<long> ChildTreeRootIds { get; set; }
        public DbTreeRepository(long id, string name) : base(id, name)
        {
            ChildTreeRootIds = new List<long>();
        }
        public DbTreeRepository()
        {
            
        }
    }
}
