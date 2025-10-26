using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Factories;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    public static class RepositoryInfrastructureConverter
    {
        public static TreeRepository ToDbEntity(this TreeRepositoryModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = businessEntity.DbEntity;
            result.Guid = businessEntity.Guid;
            result.Name = businessEntity.Name;
            result.Description = businessEntity.Description;
            result.AuditInfo = businessEntity.AuditInfo.ToDbEntity();
            result.ChildTreeRootsGuids = businessEntity.Childs.Select(x => x.Guid).ToArray();
            result.OwnDataStorageGuid = businessEntity.OwnDataStorage.Guid;
            result.DataStoragesGuids = businessEntity.DataStorages.Select(x => x.Guid).ToArray();
            return result;
        }
        public static List<TreeRepository> ToDbEntityCollection(this IEnumerable<TreeRepositoryModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeRepository>();
            foreach (var businessEntity in businessEntityCollection)
            {
                result.Add(businessEntity.ToDbEntity());
            }
            return result;
        }
        public static TreeRepositoryModel ToModel(this TreeRepository dbEntity, IEnumerable<IDataStorageModel> dataStorages)
        {
            if (dbEntity == null)
                return null;
            var dataStorage = dataStorages.FirstOrDefault(x => x.Guid == dbEntity.OwnDataStorageGuid);
            var result = new TreeRepositoryModel(dbEntity.Guid, dataStorage, dbEntity);
            result.DbEntity = dbEntity;
            result.Name = dbEntity.Name;
            result.Description = dbEntity.Description;
            result.AuditInfo = dbEntity.AuditInfo.ToModel();
            result.DataStorages = new List<IDataStorageModel>() { dataStorage };
            result.ChildsGuids = dbEntity.ChildTreeRootsGuids.ToList();
            //result.AuditInfo = dbEntity.AuditInfo.ToModel();
            //result.Childs = dbEntity.ChildTreeRoots.ToModelCollection(dataStorages);
            //result = (TreeRepositoryModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }
        public static List<TreeRepositoryModel> ToModelCollection(this IEnumerable<TreeRepository> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeRepositoryModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                result.Add(dbEntity.ToModel(dataStorages));
            }
            return result;
        }
    }
}
