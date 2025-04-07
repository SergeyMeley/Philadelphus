using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Interfaces;
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
    internal class AttributeEntryInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IDbEntity BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbAttributeEntry)BusinessToDbMainProperties((MainEntityBase)businessEntity, new DbAttributeEntry());
            result.Guid = businessEntity.Guid;
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbAttributeEntry>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbAttributeEntry)BusinessToDbMainProperties((MainEntityBase)businessEntity, new DbAttributeEntry());
                entity.Guid = businessEntity.Guid;
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = new EntityAttributeEntry(new Guid(dbEntity.ParentGuid));
            result = (EntityAttributeEntry)DbToBusinessMainProperties(dbEntity, (MainEntityBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
            return result;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<EntityAttributeEntry>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = new EntityAttributeEntry(new Guid(dbEntity.ParentGuid));
                entity = (EntityAttributeEntry)DbToBusinessMainProperties(dbEntity, (MainEntityBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
                result.Add(entity);
            }
            return result;
        }
    }
}
