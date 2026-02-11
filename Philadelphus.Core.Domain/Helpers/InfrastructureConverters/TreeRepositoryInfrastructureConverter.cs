using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    public static class PhiladelphusRepositoryHeaderInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static PhiladelphusRepositoryHeader ToDbEntity(this PhiladelphusRepositoryHeaderModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = new PhiladelphusRepositoryHeader();
            result.Uuid = businessEntity.Uuid;
            result.Name = businessEntity.Name;
            result.Description = businessEntity.Description;
            result.OwnDataStorageName = businessEntity.OwnDataStorageName;
            result.OwnDataStorageUuid = businessEntity.OwnDataStorageUuid;
            result.LastOpening = businessEntity.LastOpening;
            result.IsFavorite = businessEntity.IsFavorite;
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
        public static List<PhiladelphusRepositoryHeader> ToDbEntityCollection(this IEnumerable<PhiladelphusRepositoryHeaderModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<PhiladelphusRepositoryHeader>();
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
        /// <returns></returns>
        public static PhiladelphusRepositoryHeaderModel ToModel(this PhiladelphusRepositoryHeader dbEntity)
        {
            if (dbEntity == null)
                return null;
            var result = new PhiladelphusRepositoryHeaderModel(dbEntity.Uuid);
            result.Name = dbEntity.Name;
            result.Description = dbEntity.Description;
            result.OwnDataStorageName = dbEntity.OwnDataStorageName;
            result.OwnDataStorageUuid = dbEntity.OwnDataStorageUuid;
            result.LastOpening = dbEntity.LastOpening;
            result.IsFavorite = dbEntity.IsFavorite;
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <returns></returns>
        public static List<PhiladelphusRepositoryHeaderModel> ToModelCollection(this IEnumerable<PhiladelphusRepositoryHeader> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<PhiladelphusRepositoryHeaderModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                result.Add(dbEntity.ToModel());
            }
            return result;
        }
    }
}
