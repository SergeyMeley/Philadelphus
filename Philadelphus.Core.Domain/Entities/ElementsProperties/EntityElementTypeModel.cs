using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.ElementsProperties
{
    public class EntityElementTypeModel : MainEntityBaseModel
    {
        public override EntityTypesModel EntityType => EntityTypesModel.RepositoryElementType;

        public override IDataStorageModel DataStorage => throw new NotImplementedException();

        public EntityElementTypeModel(Guid guid, ITypedModel parent, IMainEntity dbEntity) : base(guid, dbEntity)
        {
            Name = "TEST TYPE";
        }

    }
}
