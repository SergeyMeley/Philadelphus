using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.InfrastructureConverters.Converters;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    public static class TreeRepositoryHeaderInfrastructureConverter
    {
        public static TreeRepositoryHeader ToDbEntity(this TreeRepositoryHeaderModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = new TreeRepositoryHeader();
            result.Guid = businessEntity.Guid;
            result.Name = businessEntity.Name;
            result.Description = businessEntity.Description;
            result.OwnDataStorageName = businessEntity.OwnDataStorageName;
            result.OwnDataStorageUuid = businessEntity.OwnDataStorageUuid;
            result.LastOpening = businessEntity.LastOpening;
            result.IsFavorite = businessEntity.IsFavorite;
            return result;
        }
        public static List<TreeRepositoryHeader> ToDbEntityCollection(this IEnumerable<TreeRepositoryHeaderModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeRepositoryHeader>();
            foreach (var businessEntity in businessEntityCollection)
            {
                result.Add(businessEntity.ToDbEntity());
            }
            return result;
        }
        public static TreeRepositoryHeaderModel ToModel(this TreeRepositoryHeader dbEntity)
        {
            if (dbEntity == null)
                return null;
            var result = new TreeRepositoryHeaderModel();
            result.Guid = dbEntity.Guid;
            result.Name = dbEntity.Name;
            result.Description = dbEntity.Description;
            result.OwnDataStorageName = dbEntity.OwnDataStorageName;
            result.OwnDataStorageUuid = dbEntity.OwnDataStorageUuid;
            result.LastOpening = dbEntity.LastOpening;
            result.IsFavorite = dbEntity.IsFavorite;
            return result;
        }
        public static List<TreeRepositoryHeaderModel> ToModelCollection(this IEnumerable<TreeRepositoryHeader> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeRepositoryHeaderModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                result.Add(dbEntity.ToModel());
            }
            return result;
        }
    }
}
