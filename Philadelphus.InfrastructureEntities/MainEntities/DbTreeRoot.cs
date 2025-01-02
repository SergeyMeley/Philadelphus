using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    
    public class DbTreeRoot : DbEntityBase
    {
        public long ParentTreeRepositoryId { get; set; }
        public string DirectoryPath { get; set; }
        public IEnumerable<long> AttributeIds { get; set; }
        public IEnumerable<long> ChildTreeNodeIds { get; set; }
        public DbTreeRoot(long id, string name) : base (id, name)
        {
            AttributeIds = new List<long>();
            ChildTreeNodeIds = new List<long>();
        }
}
}
