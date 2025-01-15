using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers
{
    public static class InfrastructureConverter
    {
        #region [Database to business entity]
        private static IMainEntity DbToBusinessMainProperties(IDbEntity dbEntity)
        {
            var businessEntity = new TreeRepository(dbEntity.Name, dbEntity.Id);
            businessEntity.Id = dbEntity.Id;
            businessEntity.Path = dbEntity.Path;
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
        internal static TreeRepository DbToBusinessRepository(DbTreeRepository repository)
        {
            var result = (TreeRepository)DbToBusinessMainProperties(repository);
            return result;
        }
        internal static TreeRoot DbToBusinessRoot(DbTreeRoot root)
        {
            var result = (TreeRoot)DbToBusinessMainProperties(root);
            return result;
        }
        #endregion
        #region [Business to database entity]
        private static IDbEntity BusinessToDbMainProperties(IMainEntity businessEntity)
        {
            var dbEntity = new DbTreeRepository();
            dbEntity.Id = businessEntity.Id;
            dbEntity.Path = businessEntity.Path;
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
        internal static DbTreeRepository BusinessToDbRepository(TreeRepository repository)
        {
            var result = (DbTreeRepository)BusinessToDbMainProperties(repository);
            return result;
        }
        internal static DbTreeRoot BusinessToDbRoot(TreeRoot repository)
        {
            var result = (DbTreeRoot)BusinessToDbMainProperties(repository);
            return result;
        }
        #endregion
    }
}
