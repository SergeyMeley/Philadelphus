using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public abstract class TreeRootMemberBase : MainEntityBase
    {
        public Guid? ParentGuid { get; set; }
        public abstract IMainEntity Parent { get; set; }
    }
}
