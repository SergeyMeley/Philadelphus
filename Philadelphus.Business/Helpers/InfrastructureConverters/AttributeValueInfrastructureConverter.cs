using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Factories;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal class AttributeValueInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IDbEntity BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbAttributeValue)BusinessToDbMainProperties(businessEntity, new DbAttributeValue());
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbAttributeValue>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbAttributeValue)BusinessToDbMainProperties(businessEntity, new DbAttributeValue());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = new AttributeValue(new Guid(dbEntity.ParentGuid));
            result = (AttributeValue)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            return result;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<AttributeValue>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = new AttributeValue(new Guid(dbEntity.ParentGuid));
                entity = (AttributeValue)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
                result.Add(entity);
            }
            return result;
        }
    }
}
