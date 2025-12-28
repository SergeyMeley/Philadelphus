using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent
{
    public class ElementAttributeValueModel : MainEntityBaseModel, ITreeElementContentModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.None; }
        public TreeRepositoryMemberBaseModel Owner { get; private set; }
        IAttributeOwnerModel ITreeElementContentModel.Owner => throw new NotImplementedException();

        public override IDataStorageModel DataStorage => throw new NotImplementedException();

        public ElementAttributeValueModel(Guid guid, IMainEntity dbEntity) : base(guid, dbEntity)
        {

        }
    }
}
