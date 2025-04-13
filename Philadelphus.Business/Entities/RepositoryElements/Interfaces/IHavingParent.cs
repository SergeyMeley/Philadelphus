using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    public interface IHavingParent : ILinkableByGuid
    {
        public IHavingChilds Parent { get; }
        //public Guid ParentGuid { get; }
    }
}
