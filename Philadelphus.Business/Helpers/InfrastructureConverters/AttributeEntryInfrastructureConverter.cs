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
    internal class AttributeEntryInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IDbEntity BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            var result = (DbAttributeEntry)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbAttributeEntry());
            result.Guid = businessEntity.Guid;
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            var result = new List<DbAttributeEntry>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbAttributeEntry)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbAttributeEntry());
                entity.Guid = businessEntity.Guid;
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntityModel DbToBusinessEntity(IDbEntity dbEntity)
        {
            ////var result = new EntityAttributeEntry();
            //result = (EntityAttributeEntry)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            //var result = new List<EntityAttributeEntry>();
            //foreach (var dbEntity in dbEntityCollection)
            //{
            //    //var entity = new EntityAttributeEntry());
            //    //entity = (EntityAttributeEntry)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
            //    //result.Add(entity);
            //}
            //return result;
            return null;
        }
    }
}
