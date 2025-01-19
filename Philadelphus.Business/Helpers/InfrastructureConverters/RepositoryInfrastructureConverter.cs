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
    internal class RepositoryInfrastructureConverter : InfrastructureConverterBase
    {
        internal override DbTreeRepository BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbTreeRepository)BusinessToDbMainProperties(businessEntity, new DbTreeRepository());
            return result;
        }
        internal override List<DbTreeRepository> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbTreeRepository>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbTreeRepository)BusinessToDbMainProperties(businessEntity, new DbTreeRepository());
                result.Add(entity);
            }
            return result;
        }
        internal override TreeRepository DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = (TreeRepository)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Repository));
            return result;
        }
        internal override List<TreeRepository> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<TreeRepository>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = (TreeRepository)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Repository));
                result.Add(entity);
            }
            return result;
        }
    }
}
