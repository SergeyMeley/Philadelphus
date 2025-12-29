using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.MainEntities
{
    public abstract class TreeRootMemberBase : MainEntityBase
    {
        public Guid? ParentTreeRootUuid { get; set; }
        public virtual TreeRoot ParentTreeRoot { get; set; }
        public Guid? ParentUuid { get; set; }
        public virtual IMainEntity Parent
        {
            get
            {
                return ParentTreeRoot;
            }
            set
            {
                ParentTreeRoot = (TreeRoot)value;
            }
        }
    }
}
