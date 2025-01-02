using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbTreeRepository : DbEntityBase
    {
        public string DirectoryPath { get; set; }
        public IEnumerable<long> AttributeIds { get; set; }
        public IEnumerable<long> ChildTreeRootIds { get; set; }
        public DbTreeRepository(long id, string name) : base(id, name)
        {
            AttributeIds = new List<long>();
            ChildTreeRootIds = new List<long>();
        }
    }
}
