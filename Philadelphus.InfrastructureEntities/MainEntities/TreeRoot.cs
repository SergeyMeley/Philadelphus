using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{

    public class TreeRoot : TreeRepositoryMemberBase
    {
        public Guid OwnDataStorageGuid { get; set; }
        public Guid ParentTreeRepositoryGuid { get; set; }
        public List<long> AttributeGuids { get; set; } = new List<long>();
        public List<long> ChildTreeNodeGuids { get; set; } = new List<long>();
        public TreeRoot()
        {
            
        }
    }
}
