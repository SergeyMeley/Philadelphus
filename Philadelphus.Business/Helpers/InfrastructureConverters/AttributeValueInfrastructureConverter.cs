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
using Philadelphus.Business.Helpers.InfrastructureConverters;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    public static class AttributeValueInfrastructureConverter
    {
        public static TreeElementAttributeValue BusinessToDbEntity(ElementAttributeValueModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeElementAttributeValue)businessEntity.ToDbEntityGeneralProperties(new TreeElementAttributeValue());
            return result;
        }
        public static IEnumerable<TreeElementAttributeValue> BusinessToDbEntityCollection(IEnumerable<ElementAttributeValueModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeElementAttributeValue>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (TreeElementAttributeValue)businessEntity.ToDbEntityGeneralProperties(new TreeElementAttributeValue());
                result.Add(entity);
            }
            return result;
        }
        public static ElementAttributeValueModel DbToBusinessEntity(TreeElementAttributeValue dbEntity)
        {
            if (dbEntity == null)
                return null;
            //var result = new EntityAttributeValue(dbEntity.Parent);
            //result = (EntityAttributeValue)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            //return result;
            return null;
        }
    public static IEnumerable<ElementAttributeValueModel> DbToBusinessEntityCollection(IEnumerable<TreeElementAttributeValue> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<ElementAttributeValueModel>();
            //    foreach (var dbEntity in dbEntityCollection)
            //    {
            //        var entity = new EntityAttributeValue(dbEntity.Parent);
            //        entity = (EntityAttributeValue)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
            //        result.Add(entity);
            //    }
            return result;
        }
    }
}
