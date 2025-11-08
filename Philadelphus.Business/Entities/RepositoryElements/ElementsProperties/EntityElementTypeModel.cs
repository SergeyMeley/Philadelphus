using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.ElementProperties
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
