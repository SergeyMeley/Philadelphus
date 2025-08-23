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
        internal override IDbEntity BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            var result = (DbEntityAttribute)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbEntityAttribute());
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            var result = new List<DbEntityAttribute>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbEntityAttribute)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbEntityAttribute());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntityModel DbToBusinessEntity(IDbEntity dbEntity)
        {
            //var result = new ElementAttribute(new Guid(dbEntity.ParentGuid));
            //result = (ElementAttribute)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
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
