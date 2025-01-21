using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbTreeRepository : DbEntityBase
    {
        public Guid Guid { get; set; }
        public List<long> ChildTreeRootGuids { get; set; } = new List<long>();
        public DbTreeRepository()
        {
            
        }
    }
}
