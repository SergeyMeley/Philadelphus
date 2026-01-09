using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    internal static class AuditInfoInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static AuditInfo ToDbEntity(this AuditInfoModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = new AuditInfo();
            result.CreatedAt = businessEntity.CreatedAt;
            result.CreatedBy = businessEntity.CreatedBy;
            result.UpdatedAt = businessEntity.UpdatedAt;
            result.UpdatedBy = businessEntity.UpdatedBy;
            result.ContentUpdatedAt = businessEntity.ContentUpdatedAt;
            result.ContentUpdatedBy = businessEntity.ContentUpdatedBy;
            result.IsDeleted = businessEntity.IsDeleted;
            result.DeletedAt = businessEntity.DeletedAt;
            result.DeletedBy = businessEntity.DeletedBy;
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
        public static List<AuditInfo> ToDbEntityCollection(this IEnumerable<AuditInfoModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<AuditInfo>();
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
        public static AuditInfoModel ToModel(this AuditInfo dbEntity)
        {
            if (dbEntity == null)
                return null;
            var result = new AuditInfoModel();
            result.CreatedAt = dbEntity.CreatedAt;
            result.CreatedBy = dbEntity.CreatedBy;
            result.UpdatedAt = dbEntity.UpdatedAt;
            result.UpdatedBy = dbEntity.UpdatedBy;
            result.ContentUpdatedAt = dbEntity.ContentUpdatedAt;
            result.ContentUpdatedBy = dbEntity.ContentUpdatedBy;
            result.IsDeleted = dbEntity.IsDeleted;
            result.DeletedAt = dbEntity.DeletedAt;
            result.DeletedBy = dbEntity.DeletedBy;
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <returns></returns>
        public static List<AuditInfoModel> ToModelCollection(this IEnumerable<AuditInfo> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<AuditInfoModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                result.Add(dbEntity.ToModel());
            }
            return result;
        }
    }
}
