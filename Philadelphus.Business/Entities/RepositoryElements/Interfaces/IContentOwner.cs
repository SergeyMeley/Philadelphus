using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    public interface IContentOwner : ILinkableByGuid
    {
        public bool HasContent { get; set; }
        public IEnumerable<ElementAttribute> PersonalAttributes { get; }
        public IEnumerable<ElementAttribute> ParentElementAttributes { get; }
        public IEnumerable<ElementAttributeValue> AttributeValues { get; set; }
    }
}
