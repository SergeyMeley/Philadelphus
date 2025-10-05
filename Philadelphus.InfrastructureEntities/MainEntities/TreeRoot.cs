using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{

    public class TreeRoot : MainEntityBase
    {
        public Guid OwnDataStorageGuid { get; set; }
        public Guid[] DataStoragesGuids { get; set; }
        public virtual TreeNode[] ChildTreeNodes { get; set; }
        public TreeRoot()
        {
            
        }
    }
}
