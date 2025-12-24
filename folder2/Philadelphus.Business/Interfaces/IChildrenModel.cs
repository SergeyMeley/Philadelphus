using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Interfaces
{
    public interface IChildrenModel : ILinkableByGuidModel
    {
        public IParentModel Parent { get; }
        //public Guid ParentGuid { get; }
    }
}
