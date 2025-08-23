using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
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
        internal override DbTreeRepository BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            var result = (DbTreeRepository)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbTreeRepository());
            return result;
        }
        internal override List<DbTreeRepository> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            var result = new List<DbTreeRepository>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbTreeRepository)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbTreeRepository());
                result.Add(entity);
            }
            return result;
        }
        internal override TreeRepositoryModel DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = new TreeRepositoryModel(new Guid(dbEntity.ParentGuid));
            result = (TreeRepositoryModel)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBaseModel)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypesModel.Repository));
            return result;
        }
        internal override List<TreeRepositoryModel> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<TreeRepositoryModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = new TreeRepositoryModel(new Guid(dbEntity.ParentGuid));
                entity = (TreeRepositoryModel)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBaseModel)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypesModel.Repository));
                result.Add(entity);
            }
            return result;
        }
    }
}
