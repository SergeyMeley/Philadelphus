using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Properties
{
    public class EntityElementTypeModel : MainEntityBaseModel
    {
        public override EntityTypesModel EntityType => EntityTypesModel.RepositoryElementType;

        public override IDataStorageModel DataStorage => throw new NotImplementedException();

        public EntityElementTypeModel(Guid uuid, ITypedModel parent, IMainEntity dbEntity) : base(uuid, dbEntity)
        {
            Name = "TEST TYPE";
        }

    }
}
