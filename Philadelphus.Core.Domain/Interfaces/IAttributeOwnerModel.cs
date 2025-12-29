using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface IAttributeOwnerModel : IMainEntityModel, ILinkableByUuidModel
    {
        public bool HasAttributes { get; }
        public List<ElementAttributeModel> Attributes { get; }
        public List<ElementAttributeModel> PersonalAttributes { get; }
        public List<ElementAttributeModel> ParentElementAttributes { get; }
    }
}
