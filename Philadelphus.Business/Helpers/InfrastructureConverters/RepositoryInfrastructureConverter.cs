using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
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
    public static class RepositoryInfrastructureConverter
    {
        public static TreeRepository BusinessToDbEntity(this TreeRepositoryModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeRepository)businessEntity.BusinessToDbMainProperties(new TreeRepository());
            return result;
        }
        public static List<TreeRepository> BusinessToDbEntityCollection(this IEnumerable<TreeRepositoryModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeRepository>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (TreeRepository)businessEntity.BusinessToDbMainProperties(new TreeRepository());
                result.Add(entity);
            }
            return result;
        }
        public static TreeRepositoryModel DbToBusinessEntity(this TreeRepository dbEntity, IEnumerable<IDataStorageModel> dataStorages)
        {
            if (dbEntity == null)
                return null;
            var dataStorage = dataStorages.FirstOrDefault(x => x.Guid == dbEntity.OwnDataStorageGuid);
            var result = (TreeRepositoryModel)dbEntity.DbToBusinessMainProperties(new TreeRepositoryModel(dbEntity.Guid, dataStorage));
            return result;
        }
        public static List<TreeRepositoryModel> DbToBusinessEntityCollection(this IEnumerable<TreeRepository> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeRepositoryModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var dataStorage = dataStorages.FirstOrDefault(x => x.Guid == dbEntity.OwnDataStorageGuid);
                var entity = (TreeRepositoryModel)dbEntity.DbToBusinessMainProperties(new TreeRepositoryModel(dbEntity.Guid, dataStorage));
                result.Add(entity);
            }
            return result;
        }
    }
}
