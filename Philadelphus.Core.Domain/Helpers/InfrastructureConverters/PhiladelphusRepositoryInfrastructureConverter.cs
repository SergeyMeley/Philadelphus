using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    public static class PhiladelphusRepositoryInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static PhiladelphusRepository ToDbEntity(this PhiladelphusRepositoryModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = businessEntity.DbEntity as PhiladelphusRepository;
            result.Uuid = businessEntity.Uuid;
            result.Name = businessEntity.Name;
            result.Description = businessEntity.Description;
            result.AuditInfo = businessEntity.AuditInfo.ToDbEntity();
            result.ContentWorkingTreesUuids = businessEntity.ContentShrub.ContentTreesUuids.ToArray();
            result.OwnDataStorageUuid = businessEntity.OwnDataStorage.Uuid;
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
        public static List<PhiladelphusRepository> ToDbEntityCollection(this IEnumerable<PhiladelphusRepositoryModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<PhiladelphusRepository>();
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
        public static PhiladelphusRepositoryModel ToModel(this PhiladelphusRepository dbEntity, IDataStorageModel dataStorage)
        {
            if (dbEntity == null)
                return null;
            if (dataStorage == null)
                throw new ArgumentNullException();
            var result = new PhiladelphusRepositoryModel(dbEntity.Uuid, dataStorage, dbEntity);
            result.DbEntity = dbEntity;
            result.Name = dbEntity.Name;
            result.Description = dbEntity.Description;
            result.AuditInfo = dbEntity.AuditInfo.ToModel();
            result.ContentShrub.ContentTreesUuids = dbEntity.ContentWorkingTreesUuids.ToList();
            result.AuditInfo = dbEntity.AuditInfo.ToModel();
            result = (PhiladelphusRepositoryModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }


        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="dataStorages">Коллекция доступных хранилищ данных</param>
        /// <returns></returns>
        public static List<PhiladelphusRepositoryModel> ToModelCollection(this IEnumerable<PhiladelphusRepository> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages)
        {
            if (dbEntityCollection == null)
                return null;
            if (dbEntityCollection.Count() == 0)
                return new List<PhiladelphusRepositoryModel>();
            if (dataStorages == null || dataStorages.Count() == 0)
                throw new ArgumentNullException();
            var result = new List<PhiladelphusRepositoryModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var dataStorage = dataStorages.FirstOrDefault(x => x.Uuid == dbEntity.OwnDataStorageUuid);
                result.Add(dbEntity.ToModel(dataStorage));
            }
            return result;
        }
    }
}
