using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.MainEntities
{

    public class TreeRoot : MainEntityBase
    {
        public Guid OwnDataStorageUuid { get; set; }
        public Guid[] DataStoragesUuids { get; set; }   //TODO: Удалить
        public virtual TreeNode[] ChildTreeNodes { get; set; }
        public TreeRoot()
        {
            
        }
    }
}
