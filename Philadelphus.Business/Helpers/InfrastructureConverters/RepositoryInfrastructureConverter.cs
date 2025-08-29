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
        internal override TreeRepository BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeRepository)BusinessToDbMainProperties((MainEntityBaseModel)businessEntity, new TreeRepository());
            return result;
        }
        internal override List<TreeRepository> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeRepository>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (TreeRepository)BusinessToDbMainProperties((MainEntityBaseModel)businessEntity, new TreeRepository());
                result.Add(entity);
            }
            return result;
        }
        internal override TreeRepositoryModel DbToBusinessEntity(IEntity dbEntity)
        {
            if (dbEntity == null)
                return null;
            var result = new TreeRepositoryModel(new Guid(dbEntity.ParentGuid));
            result = (TreeRepositoryModel)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBaseModel)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypesModel.Repository, new Guid(dbEntity.ParentGuid)));
            return result;
        }
        internal override List<TreeRepositoryModel> DbToBusinessEntityCollection(IEnumerable<IEntity> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeRepositoryModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = new TreeRepositoryModel(Guid.Empty);
                entity = (TreeRepositoryModel)DbToBusinessMainProperties(dbEntity, (MainEntityBaseModel)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypesModel.Repository, Guid.Empty));
                result.Add(entity);
            }
            return result;
        }
    }
}
