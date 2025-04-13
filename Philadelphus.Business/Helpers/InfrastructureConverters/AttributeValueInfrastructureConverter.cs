using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
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
            var result = (DbAttributeValue)BusinessToDbMainProperties((RepositoryElementBase)businessEntity, new DbAttributeValue());
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbAttributeValue>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbAttributeValue)BusinessToDbMainProperties((RepositoryElementBase)businessEntity, new DbAttributeValue());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            //var result = new EntityAttributeValue(dbEntity.Parent);
            //result = (EntityAttributeValue)DbToBusinessMainProperties(dbEntity, (RepositoryElementBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<EntityAttributeValue>();
            //    foreach (var dbEntity in dbEntityCollection)
            //    {
            //        var entity = new EntityAttributeValue(dbEntity.Parent);
            //        entity = (EntityAttributeValue)DbToBusinessMainProperties(dbEntity, (RepositoryElementBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            //        result.Add(entity);
            //    }
            return result;
        }
    }
}
