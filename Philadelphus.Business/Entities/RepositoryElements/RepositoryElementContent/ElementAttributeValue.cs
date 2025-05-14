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
    public class ElementAttributeValue : MainEntityBase, ITreeElementContent
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public TreeRepositoryMemberBase Owner { get; private set; }
        IContentOwner ITreeElementContent.Owner => throw new NotImplementedException();
        public ElementAttributeValue(Guid guid) : base(guid)
        {

        }
    }
}
