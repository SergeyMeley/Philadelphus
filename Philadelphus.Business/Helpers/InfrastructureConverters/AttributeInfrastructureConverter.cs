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
    internal class AttributeInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IEntity BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            var result = (EntityAttribute)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new EntityAttribute());
            return result;
        }
        internal override IEnumerable<IEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            var result = new List<EntityAttribute>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (EntityAttribute)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new EntityAttribute());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntityModel DbToBusinessEntity(IEntity dbEntity)
        {
            //var result = new ElementAttribute(new Guid(dbEntity.ParentGuid));
            //result = (ElementAttribute)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IEntity> dbEntityCollection)
        {
            var result = new List<OLD_ElementAttributeModel>();
            //foreach (var dbEntity in dbEntityCollection)
            //{
            //    var entity = new ElementAttribute(new Guid(dbEntity.ParentGuid));
            //    entity = (ElementAttribute)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
            //    result.Add(entity);
            //}
            return result;
        }
    }
}
