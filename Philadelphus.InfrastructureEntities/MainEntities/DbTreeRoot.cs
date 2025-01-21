using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{

    public class DbTreeRoot : DbEntityBase
    {
        public Guid Guid { get; set; }
        public long ParentTreeRepositoryGuid { get; set; }
        public List<long> AttributeGuids { get; set; } = new List<long>();
        public List<long> ChildTreeNodeGuids { get; set; } = new List<long>();
        public DbTreeRoot()
        {
            
        }
    }
}
