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
        internal override IEntity BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            var result = (AttributeValue)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeValue());
            return result;
        }
        internal override IEnumerable<IEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            var result = new List<AttributeValue>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (AttributeValue)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeValue());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntityModel DbToBusinessEntity(IEntity dbEntity)
        {
            //var result = new EntityAttributeValue(dbEntity.Parent);
            //result = (EntityAttributeValue)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IEntity> dbEntityCollection)
        {
            var result = new List<ElementAttributeValueModel>();
            //    foreach (var dbEntity in dbEntityCollection)
            //    {
            //        var entity = new EntityAttributeValue(dbEntity.Parent);
            //        entity = (EntityAttributeValue)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            //        result.Add(entity);
            //    }
            return result;
        }
    }
}
