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
    internal class AttributeInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IDbEntity BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbEntityAttribute)BusinessToDbMainProperties(businessEntity, new DbEntityAttribute());
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbEntityAttribute>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbEntityAttribute)BusinessToDbMainProperties(businessEntity, new DbEntityAttribute());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = new EntityAttribute(new Guid(dbEntity.ParentGuid));
            result = (EntityAttribute)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
            return result;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<EntityAttribute>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = new EntityAttribute(new Guid(dbEntity.ParentGuid));
                entity = (EntityAttribute)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
                result.Add(entity);
            }
            return result;
        }
    }
}
