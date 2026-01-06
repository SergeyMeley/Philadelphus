using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    public static class RepositoryInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static TreeRepository ToDbEntity(this TreeRepositoryModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = businessEntity.DbEntity;
            result.Uuid = businessEntity.Uuid;
            result.Name = businessEntity.Name;
            result.Description = businessEntity.Description;
            result.AuditInfo = businessEntity.AuditInfo.ToDbEntity();
            result.ChildTreeRootsUuids = businessEntity.Childs.Select(x => x.Uuid).ToArray();
            result.OwnDataStorageUuid = businessEntity.OwnDataStorage.Uuid;
            result.DataStoragesUuids = businessEntity.DataStorages.Select(x => x.Uuid).ToArray();
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
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

        /// <summary>
        /// Конвертировать сущность БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="dataStorages">Доступные хранилища данных</param>
        /// <returns></returns>
        public static TreeRepositoryModel ToModel(this TreeRepository dbEntity, IEnumerable<IDataStorageModel> dataStorages)
        {
            if (dbEntity == null)
                return null;
            var dataStorage = dataStorages.FirstOrDefault(x => x.Uuid == dbEntity.OwnDataStorageUuid);
            var result = new TreeRepositoryModel(dbEntity.Uuid, dataStorage, dbEntity);
            result.DbEntity = dbEntity;
            result.Name = dbEntity.Name;
            result.Description = dbEntity.Description;
            result.AuditInfo = dbEntity.AuditInfo.ToModel();
            result.DataStorages = new List<IDataStorageModel>() { dataStorage };
            result.ChildsUuids = dbEntity.ChildTreeRootsUuids.ToList();
            //result.AuditInfo = dbEntity.AuditInfo.ToModel();
            //result.Childs = dbEntity.ChildTreeRoots.ToModelCollection(dataStorages);
            //result = (TreeRepositoryModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }


        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="dataStorages">Коллекция доступных хранилищ данных</param>
        /// <returns></returns>
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
