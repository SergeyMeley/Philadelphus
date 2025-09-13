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
        internal override IMainEntity BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeElementAttributeValue)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeElementAttributeValue());
            return result;
        }
        internal override IEnumerable<IMainEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeElementAttributeValue>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (TreeElementAttributeValue)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeElementAttributeValue());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntityModel DbToBusinessEntity(IMainEntity dbEntity)
        {
            if (dbEntity == null)
                return null;
            //var result = new EntityAttributeValue(dbEntity.Parent);
            //result = (EntityAttributeValue)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IMainEntity> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
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
