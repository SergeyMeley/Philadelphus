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
        protected static IMainEntity DbToBusinessMainProperties(IDbEntity dbEntity, IMainEntity businessEntity)
        {
            businessEntity.ParentGuid = dbEntity.ParentGuid;
            businessEntity.DbId = dbEntity.Id;
            businessEntity.DirectoryPath = dbEntity.DirectoryPath;
            businessEntity.DirectoryFullPath = dbEntity.DirectoryFullPath;
            businessEntity.ConfigPath = dbEntity.ConfigPath;
            businessEntity.Sequence = dbEntity.Sequence;
            businessEntity.Name = dbEntity.Name;
            businessEntity.Alias = dbEntity.Alias;
            businessEntity.Code = dbEntity.Code;
            businessEntity.Description = dbEntity.Description;
            businessEntity.HasContent = dbEntity.HasContent;
            businessEntity.IsOriginal = dbEntity.IsOriginal;
            businessEntity.IsLegacy = dbEntity.IsLegacy;
            businessEntity.IsDeleted = dbEntity.IsDeleted;
            businessEntity.CreatedOn = dbEntity.CreatedOn;
            businessEntity.CreatedBy = dbEntity.CreatedBy;
            businessEntity.UpdatedOn = dbEntity.UpdatedOn;
            businessEntity.UpdatedBy = dbEntity.UpdatedBy;
            businessEntity.UpdatedContentOn = dbEntity.UpdatedContentOn;
            businessEntity.UpdatedContentBy = dbEntity.UpdatedContentBy;
            businessEntity.DeletedOn = dbEntity.DeletedOn;
            businessEntity.DeletedBy = dbEntity.DeletedBy;
            return businessEntity;
        }
        internal abstract IMainEntity DbToBusinessEntity(IDbEntity dbEntity);
        internal abstract IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection);
        protected static IDbEntity BusinessToDbMainProperties(IMainEntity businessEntity, IDbEntity dbEntity)
        {
            dbEntity.Guid = businessEntity.Guid;
            dbEntity.ParentGuid = businessEntity.ParentGuid;
            dbEntity.Id = businessEntity.DbId;
            dbEntity.DirectoryPath = businessEntity.DirectoryPath;
            dbEntity.DirectoryFullPath = businessEntity.DirectoryFullPath;
            dbEntity.ConfigPath = businessEntity.ConfigPath;
            dbEntity.Sequence = businessEntity.Sequence;
            dbEntity.Name = businessEntity.Name;
            dbEntity.Alias = businessEntity.Alias;
            dbEntity.Code = businessEntity.Code;
            dbEntity.Description = businessEntity.Description;
            dbEntity.HasContent = businessEntity.HasContent;
            dbEntity.IsOriginal = businessEntity.IsOriginal;
            dbEntity.IsLegacy = businessEntity.IsLegacy;
            dbEntity.IsDeleted = businessEntity.IsDeleted;
            dbEntity.CreatedOn = businessEntity.CreatedOn;
            dbEntity.CreatedBy = businessEntity.CreatedBy;
            dbEntity.UpdatedOn = businessEntity.UpdatedOn;
            dbEntity.UpdatedBy = businessEntity.UpdatedBy;
            dbEntity.UpdatedContentOn = businessEntity.UpdatedContentOn;
            dbEntity.UpdatedContentBy = businessEntity.UpdatedContentBy;
            dbEntity.DeletedOn = businessEntity.DeletedOn;
            dbEntity.DeletedBy = businessEntity.DeletedBy;
            return dbEntity;
        }
        internal abstract IDbEntity BusinessToDbEntity(IMainEntity businessEntity);
        internal abstract IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection);
    }
}
