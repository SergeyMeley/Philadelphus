using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    public interface IContentOwnerModel : ILinkableByGuidModel
    {
        public bool HasContent { get; set; }
        public List<ElementAttributeModel> PersonalAttributes { get; }
        public List<ElementAttributeModel> ParentElementAttributes { get; }
        public List<ElementAttributeValueModel> AttributeValues { get; set; }
    }
}
