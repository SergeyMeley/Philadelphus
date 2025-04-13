using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    internal interface IHavingContent : ILinkableByGuid
    {
        public bool HasContent { get; set; }
    }
}
