using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    internal static class InfrastructureConverterBase
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IMainEntity ToDbEntity(this IMainEntityModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            IMainEntity result = null;
            if (businessEntity.GetType().IsAssignableTo(typeof(TreeRootModel)))
            {
                TreeRootModel model = (TreeRootModel)businessEntity;
                return model.ToDbEntity();
            }
            if (businessEntity.GetType().IsAssignableTo(typeof(TreeNodeModel)))
            {
                TreeNodeModel model = (TreeNodeModel)businessEntity;
                return model.ToDbEntity();
            }
            if (businessEntity.GetType().IsAssignableTo(typeof(TreeLeaveModel)))
            {
                TreeLeaveModel model = (TreeLeaveModel)businessEntity;
                return model.ToDbEntity();
            }
            throw new Exception();
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
        public static List<IMainEntity> ToDbEntityCollection(this IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            List<IMainEntity> result = null;
            foreach (var item in businessEntityCollection)
            {
                result.Add(item.ToDbEntity());
            }
            return result;
        }

        /// <summary>
        /// Конвертировать основные свойства доменной модели в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <param name="dbEntity">Сущность БД</param>
        /// <returns></returns>
        public static IMainEntity ToDbEntityGeneralProperties(this MainEntityBaseModel businessEntity, IMainEntity dbEntity)
        {
            if (businessEntity == null)
                return null;
            //dbEntity.ParentUuid = businessEntity.Parent.ToString();
            //dbEntity.DirectoryPath = businessEntity.DirectoryPath;
            //dbEntity.DirectoryFullPath = businessEntity.DirectoryFullPath;
            //dbEntity.ConfigPath = businessEntity.ConfigPath;
            dbEntity.Uuid = businessEntity.Uuid;
            dbEntity.Name = businessEntity.Name;
            dbEntity.Alias = businessEntity.Alias;
            dbEntity.CustomCode = businessEntity.CustomCode;
            dbEntity.Description = businessEntity.Description;
            dbEntity.IsLegacy = businessEntity.IsLegacy;
            dbEntity.AuditInfo.IsDeleted = businessEntity.AuditInfo.IsDeleted;
            dbEntity.AuditInfo.CreatedAt = businessEntity.AuditInfo.CreatedAt;
            dbEntity.AuditInfo.CreatedBy = businessEntity.AuditInfo.CreatedBy;
            dbEntity.AuditInfo.UpdatedAt = businessEntity.AuditInfo.UpdatedAt;
            dbEntity.AuditInfo.UpdatedBy = businessEntity.AuditInfo.UpdatedBy;
            dbEntity.AuditInfo.ContentUpdatedAt = businessEntity.AuditInfo.ContentUpdatedAt;
            dbEntity.AuditInfo.ContentUpdatedBy = businessEntity.AuditInfo.ContentUpdatedBy;
            dbEntity.AuditInfo.DeletedAt = businessEntity.AuditInfo.DeletedAt;
            dbEntity.AuditInfo.DeletedBy = businessEntity.AuditInfo.DeletedBy;
            return dbEntity;
        }

        /// <summary>
        /// Конвертировать сущность БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="dataStorages">Доступные хранилища данных</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IMainEntityModel ToModel(this IMainEntity dbEntity, IEnumerable<IDataStorageModel> dataStorages)
        {
            if (dbEntity == null)
                return null;
            IMainEntity result = null;
            if (dbEntity.GetType().IsAssignableTo(typeof(TreeRoot)))
            {
                TreeRoot entity = (TreeRoot)dbEntity;
                return entity.ToModel(dataStorages);
            }
            if (dbEntity.GetType().IsAssignableTo(typeof(TreeNode)))
            {
                TreeNode entity = (TreeNode)dbEntity;
                return entity.ToModel(dataStorages);
            }
            if (dbEntity.GetType().IsAssignableTo(typeof(TreeLeave)))
            {
                TreeLeave entity = (TreeLeave)dbEntity;
                return entity.ToModel(dataStorages);
            }
            throw new Exception();
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="dataStorages">Доступные хранилища данных</param>
        /// <returns></returns>
        public static List<IMainEntityModel> ToModelCollection(this IEnumerable<IMainEntity> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages)
        {
            List<IMainEntityModel> result = null;
            foreach (var item in dbEntityCollection)
            {
                result.Add(item.ToModel(dataStorages));
            }
            return result;
        }

        /// <summary>
        /// Конвертировать основные свойства сущности БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static MainEntityBaseModel ToModelGeneralProperties(this IMainEntity dbEntity, MainEntityBaseModel businessEntity)
        {
            if (dbEntity == null)
                return null;
            //businessEntity.DirectoryPath = dbEntity.DirectoryPath;
            //businessEntity.DirectoryFullPath = dbEntity.DirectoryFullPath;
            //businessEntity.ConfigPath = dbEntity.ConfigPath;
            businessEntity.DbEntity = dbEntity;
            businessEntity.Name = dbEntity.Name;
            businessEntity.Alias = dbEntity.Alias;
            businessEntity.CustomCode = dbEntity.CustomCode;
            businessEntity.Description = dbEntity.Description;
            businessEntity.IsLegacy = dbEntity.IsLegacy;
            businessEntity.AuditInfo.IsDeleted = dbEntity.AuditInfo.IsDeleted;
            businessEntity.AuditInfo.CreatedAt = dbEntity.AuditInfo.CreatedAt;
            businessEntity.AuditInfo.CreatedBy = dbEntity.AuditInfo.CreatedBy;
            businessEntity.AuditInfo.UpdatedAt = dbEntity.AuditInfo.UpdatedAt;
            businessEntity.AuditInfo.UpdatedBy = dbEntity.AuditInfo.UpdatedBy;
            businessEntity.AuditInfo.ContentUpdatedAt = dbEntity.AuditInfo.ContentUpdatedAt;
            businessEntity.AuditInfo.ContentUpdatedBy = dbEntity.AuditInfo.ContentUpdatedBy;
            businessEntity.AuditInfo.DeletedAt = dbEntity.AuditInfo.DeletedAt;
            businessEntity.AuditInfo.DeletedBy = dbEntity.AuditInfo.DeletedBy;
            return businessEntity;
        }
    }
}
