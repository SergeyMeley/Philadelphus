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
//    internal static class InfrastructureConverterBase
//    {
//        public static MainEntityBaseModel ToModelGeneralProperties(this IMainEntity dbEntity, MainEntityBaseModel businessEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //businessEntity.DirectoryPath = dbEntity.DirectoryPath;
//            //businessEntity.DirectoryFullPath = dbEntity.DirectoryFullPath;
//            //businessEntity.ConfigPath = dbEntity.ConfigPath;
//            businessEntity.Name = dbEntity.Name;
//            businessEntity.Alias = dbEntity.Alias;
//            businessEntity.CustomCode = dbEntity.CustomCode;
//            businessEntity.Description = dbEntity.Description;
//            businessEntity.HasContent = dbEntity.HasContent;
//            businessEntity.IsOriginal = dbEntity.IsOriginal;
//            businessEntity.IsLegacy = dbEntity.IsLegacy;
//            businessEntity.AuditInfo.IsDeleted = dbEntity.AuditInfo.IsDeleted;
//            businessEntity.AuditInfo.CreatedAt = dbEntity.AuditInfo.CreatedAt;
//            businessEntity.AuditInfo.CreatedBy = dbEntity.AuditInfo.CreatedBy;
//            businessEntity.AuditInfo.UpdatedAt = dbEntity.AuditInfo.UpdatedAt;
//            businessEntity.AuditInfo.UpdatedBy = dbEntity.AuditInfo.UpdatedBy;
//            businessEntity.AuditInfo.ContentUpdatedAt = dbEntity.AuditInfo.ContentUpdatedAt;
//            businessEntity.AuditInfo.ContentUpdatedBy = dbEntity.AuditInfo.ContentUpdatedBy;
//            businessEntity.AuditInfo.DeletedAt = dbEntity.AuditInfo.DeletedAt;
//            businessEntity.AuditInfo.DeletedBy = dbEntity.AuditInfo.DeletedBy;
//            return businessEntity;
//        }
//        public static IMainEntity ToDbEntityGeneralProperties(this MainEntityBaseModel businessEntity, IMainEntity dbEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            //dbEntity.ParentGuid = businessEntity.Parent.ToString();
//            //dbEntity.DirectoryPath = businessEntity.DirectoryPath;
//            //dbEntity.DirectoryFullPath = businessEntity.DirectoryFullPath;
//            //dbEntity.ConfigPath = businessEntity.ConfigPath;
//            dbEntity.Name = businessEntity.Name;
//            dbEntity.Alias = businessEntity.Alias;
//            dbEntity.CustomCode = businessEntity.CustomCode;
//            dbEntity.Description = businessEntity.Description;
//            dbEntity.HasContent = businessEntity.HasContent;
//            dbEntity.IsOriginal = businessEntity.IsOriginal;
//            dbEntity.IsLegacy = businessEntity.IsLegacy;
//            dbEntity.AuditInfo.IsDeleted = businessEntity.AuditInfo.IsDeleted;
//            dbEntity.AuditInfo.CreatedAt = businessEntity.AuditInfo.CreatedAt;
//            dbEntity.AuditInfo.CreatedBy = businessEntity.AuditInfo.CreatedBy;
//            dbEntity.AuditInfo.UpdatedAt = businessEntity.AuditInfo.UpdatedAt;
//            dbEntity.AuditInfo.UpdatedBy = businessEntity.AuditInfo.UpdatedBy;
//            dbEntity.AuditInfo.ContentUpdatedAt = businessEntity.AuditInfo.ContentUpdatedAt;
//            dbEntity.AuditInfo.ContentUpdatedBy = businessEntity.AuditInfo.ContentUpdatedBy;
//            dbEntity.AuditInfo.DeletedAt = businessEntity.AuditInfo.DeletedAt;
//            dbEntity.AuditInfo.DeletedBy = businessEntity.AuditInfo.DeletedBy;
//            return dbEntity;
//        }
//    }
//}
