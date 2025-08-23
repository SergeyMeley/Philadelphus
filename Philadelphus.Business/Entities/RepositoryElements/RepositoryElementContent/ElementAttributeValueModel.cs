using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent
{
    public class ElementAttributeValueModel : MainEntityBaseModel, ITreeElementContentModek
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.None; }
        public TreeRepositoryMemberBaseModel Owner { get; private set; }
        IContentOwnerModel ITreeElementContentModek.Owner => throw new NotImplementedException();
        public ElementAttributeValueModel(Guid guid) : base(guid)
        {

        }
    }
}
