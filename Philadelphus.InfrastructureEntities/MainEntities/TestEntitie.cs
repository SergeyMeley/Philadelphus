using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class TestEntitie : EntityBase
    {
        public long ParentTreeRepositoryId { get; set; }
        public List<long> AttributeIds { get; set; } = new List<long>();
        public List<long> ChildTreeNodeIds { get; set; } = new List<long>();
        public TestEntitie()
        {
            Guid = Guid.NewGuid();
        }
    }
}
