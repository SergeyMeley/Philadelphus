//using Philadelphus.Business.Entities.RepositoryElements;
//using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
//using Philadelphus.InfrastructureEntities.MainEntities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Philadelphus.InfrastructureConverters.Converters
//{
//    public abstract class InfrastructureConverterBase
//    {
//        protected static IMainEntityModel DbToBusinessMainProperties(IEntity dbEntity, MainEntityBaseModel businessEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //businessEntity.DirectoryPath = dbEntity.DirectoryPath;
//            //businessEntity.DirectoryFullPath = dbEntity.DirectoryFullPath;
//            //businessEntity.ConfigPath = dbEntity.ConfigPath;
//            businessEntity.Name = dbEntity.Name;
//            businessEntity.Alias = dbEntity.Alias;
//            businessEntity.CustomCode = dbEntity.Code;
//            businessEntity.Description = dbEntity.Description;
//            businessEntity.HasContent = dbEntity.HasContent;
//            businessEntity.IsOriginal = dbEntity.IsOriginal;
//            businessEntity.IsLegacy = dbEntity.IsLegacy;
//            businessEntity.AuditInfo.IsDeleted = dbEntity.AuditInfo.IsDeleted;
//            businessEntity.AuditInfo.CreatedOn = dbEntity.AuditInfo.CreatedOn;
//            businessEntity.AuditInfo.CreatedBy = dbEntity.AuditInfo.CreatedBy;
//            businessEntity.AuditInfo.UpdatedOn = dbEntity.AuditInfo.UpdatedOn;
//            businessEntity.AuditInfo.UpdatedBy = dbEntity.AuditInfo.UpdatedBy;
//            businessEntity.AuditInfo.UpdatedContentOn = dbEntity.AuditInfo.UpdatedContentOn;
//            businessEntity.AuditInfo.UpdatedContentBy = dbEntity.AuditInfo.UpdatedContentBy;
//            businessEntity.AuditInfo.DeletedOn = dbEntity.AuditInfo.DeletedOn;
//            businessEntity.AuditInfo.DeletedBy = dbEntity.AuditInfo.DeletedBy;
//            return businessEntity;
//        }
//        internal abstract IMainEntityModel DbToBusinessEntity(IEntity dbEntity);
//        internal abstract IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IEntity> dbEntityCollection);
//        protected static IEntity BusinessToDbMainProperties(MainEntityBaseModel businessEntity, IEntity dbEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            //dbEntity.ParentGuid = businessEntity.Parent.ToString();
//            //dbEntity.DirectoryPath = businessEntity.DirectoryPath;
//            //dbEntity.DirectoryFullPath = businessEntity.DirectoryFullPath;
//            //dbEntity.ConfigPath = businessEntity.ConfigPath;
//            dbEntity.Name = businessEntity.Name;
//            dbEntity.Alias = businessEntity.Alias;
//            dbEntity.Code = businessEntity.CustomCode;
//            dbEntity.Description = businessEntity.Description;
//            dbEntity.HasContent = businessEntity.HasContent;
//            dbEntity.IsOriginal = businessEntity.IsOriginal;
//            dbEntity.IsLegacy = businessEntity.IsLegacy;
//            dbEntity.AuditInfo.IsDeleted = businessEntity.AuditInfo.IsDeleted;
//            dbEntity.AuditInfo.CreatedOn = businessEntity.AuditInfo.CreatedOn;
//            dbEntity.AuditInfo.CreatedBy = businessEntity.AuditInfo.CreatedBy;
//            dbEntity.AuditInfo.UpdatedOn = businessEntity.AuditInfo.UpdatedOn;
//            dbEntity.AuditInfo.UpdatedBy = businessEntity.AuditInfo.UpdatedBy;
//            dbEntity.AuditInfo.UpdatedContentOn = businessEntity.AuditInfo.UpdatedContentOn;
//            dbEntity.AuditInfo.UpdatedContentBy = businessEntity.AuditInfo.UpdatedContentBy;
//            dbEntity.AuditInfo.DeletedOn = businessEntity.AuditInfo.DeletedOn;
//            dbEntity.AuditInfo.DeletedBy = businessEntity.AuditInfo.DeletedBy;
//            return dbEntity;
//        }
//        internal abstract IEntity BusinessToDbEntity(IMainEntityModel businessEntity);
//        internal abstract IEnumerable<IEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection);
//    }
//}
