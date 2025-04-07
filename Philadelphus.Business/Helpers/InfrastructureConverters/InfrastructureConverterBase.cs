using Philadelphus.Business.Entities.Interfaces;
using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    public abstract class InfrastructureConverterBase
    {
        protected static IMainEntity DbToBusinessMainProperties(IDbEntity dbEntity, MainEntityBase businessEntity)
        {
            businessEntity.DirectoryPath = dbEntity.DirectoryPath;
            businessEntity.DirectoryFullPath = dbEntity.DirectoryFullPath;
            businessEntity.ConfigPath = dbEntity.ConfigPath;
            businessEntity.Sequence = dbEntity.Sequence;
            businessEntity.Name = dbEntity.Name;
            businessEntity.Alias = dbEntity.Alias;
            businessEntity.CustomCode = dbEntity.Code;
            businessEntity.Description = dbEntity.Description;
            businessEntity.HasContent = dbEntity.HasContent;
            businessEntity.IsOriginal = dbEntity.IsOriginal;
            businessEntity.IsLegacy = dbEntity.IsLegacy;
            businessEntity.AuditInfo.IsDeleted = dbEntity.IsDeleted;
            businessEntity.AuditInfo.CreatedOn = dbEntity.CreatedOn;
            businessEntity.AuditInfo.CreatedBy = dbEntity.CreatedBy;
            businessEntity.AuditInfo.UpdatedOn = dbEntity.UpdatedOn;
            businessEntity.AuditInfo.UpdatedBy = dbEntity.UpdatedBy;
            businessEntity.AuditInfo.UpdatedContentOn = dbEntity.UpdatedContentOn;
            businessEntity.AuditInfo.UpdatedContentBy = dbEntity.UpdatedContentBy;
            businessEntity.AuditInfo.DeletedOn = dbEntity.DeletedOn;
            businessEntity.AuditInfo.DeletedBy = dbEntity.DeletedBy;
            return businessEntity;
        }
        internal abstract IMainEntity DbToBusinessEntity(IDbEntity dbEntity);
        internal abstract IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection);
        protected static IDbEntity BusinessToDbMainProperties(MainEntityBase businessEntity, IDbEntity dbEntity)
        {
            dbEntity.ParentGuid = businessEntity.ParentGuid.ToString();
            dbEntity.DirectoryPath = businessEntity.DirectoryPath;
            dbEntity.DirectoryFullPath = businessEntity.DirectoryFullPath;
            dbEntity.ConfigPath = businessEntity.ConfigPath;
            dbEntity.Sequence = businessEntity.Sequence;
            dbEntity.Name = businessEntity.Name;
            dbEntity.Alias = businessEntity.Alias;
            dbEntity.Code = businessEntity.CustomCode;
            dbEntity.Description = businessEntity.Description;
            dbEntity.HasContent = businessEntity.HasContent;
            dbEntity.IsOriginal = businessEntity.IsOriginal;
            dbEntity.IsLegacy = businessEntity.IsLegacy;
            dbEntity.IsDeleted = businessEntity.AuditInfo.IsDeleted;
            dbEntity.CreatedOn = businessEntity.AuditInfo.CreatedOn;
            dbEntity.CreatedBy = businessEntity.AuditInfo.CreatedBy;
            dbEntity.UpdatedOn = businessEntity.AuditInfo.UpdatedOn;
            dbEntity.UpdatedBy = businessEntity.AuditInfo.UpdatedBy;
            dbEntity.UpdatedContentOn = businessEntity.AuditInfo.UpdatedContentOn;
            dbEntity.UpdatedContentBy = businessEntity.AuditInfo.UpdatedContentBy;
            dbEntity.DeletedOn = businessEntity.AuditInfo.DeletedOn;
            dbEntity.DeletedBy = businessEntity.AuditInfo.DeletedBy;
            return dbEntity;
        }
        internal abstract IDbEntity BusinessToDbEntity(IMainEntity businessEntity);
        internal abstract IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection);
    }
}
