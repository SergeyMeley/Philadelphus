using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes
{
    public class ElementAttributeValueModel : MainEntityBaseModel, ITreeElementContentModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.None; }
        public TreeRepositoryMemberBaseModel Owner { get; private set; }
        IAttributeOwnerModel ITreeElementContentModel.Owner => throw new NotImplementedException();

        public override IDataStorageModel DataStorage => throw new NotImplementedException();

        public ElementAttributeValueModel(Guid uuid, IMainEntity dbEntity) : base(uuid, dbEntity)
        {

        }
    }
}
