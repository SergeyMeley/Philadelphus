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
        private static IMainEntity DbToBusinessMainProperties(IDbEntity dbEntity, IMainEntity businessEntity)
        {
            businessEntity.ParentGuid = dbEntity.ParentGuid;
            businessEntity.DbId = dbEntity.Id;
            businessEntity.DirectoryPath = dbEntity.DirectoryPath;
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
            var result = (TreeRepository)DbToBusinessMainProperties(repository, new TreeRepository(repository.Name, repository.Guid, repository.ParentGuid));
            return result;
        }
        internal static TreeRoot DbToBusinessRoot(DbTreeRoot root)
        {
            var result = (TreeRoot)DbToBusinessMainProperties(root, new TreeRoot(root.Name, root.ParentGuid));
            return result;
        }
        internal static TreeNode DbToBusinessNode(DbTreeNode node)
        {
            var result = (TreeNode)DbToBusinessMainProperties(node, new TreeNode(node.Name, node.ParentGuid));
            return result;
        }
        internal static TreeLeave DbToBusinessLeave(DbTreeLeave leave)
        {
            var result = (TreeLeave)DbToBusinessMainProperties(leave, new TreeLeave(leave.Name, leave.ParentGuid));
            return result;
        }
        internal static Entities.MainEntities.Attribute DbToBusinessNode(DbAttribute attribute)
        {
            var result = (Entities.MainEntities.Attribute)DbToBusinessMainProperties(attribute, new Entities.MainEntities.Attribute(attribute.Name, attribute.ParentGuid));
            return result;
        }
        #endregion
        #region [Business to database entity]
        private static IDbEntity BusinessToDbMainProperties(IMainEntity businessEntity, IDbEntity dbEntity)
        {
            dbEntity.Guid = businessEntity.Guid;
            dbEntity.ParentGuid = businessEntity.ParentGuid;
            dbEntity.Id = businessEntity.DbId;
            dbEntity.DirectoryPath = businessEntity.DirectoryPath;
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
            var result = (DbTreeRepository)BusinessToDbMainProperties(repository, new DbTreeRepository());
            return result;
        }
        internal static DbTreeRoot BusinessToDbRoot(TreeRoot root)
        {
            var result = (DbTreeRoot)BusinessToDbMainProperties(root, new DbTreeRoot());
            return result;
        }
        internal static DbTreeNode BusinessToDbNode(TreeNode node)
        {
            var result = (DbTreeNode)BusinessToDbMainProperties(node, new DbTreeNode());
            return result;
        }
        internal static DbTreeLeave BusinessToDbLeave(TreeLeave leave)
        {
            var result = (DbTreeLeave)BusinessToDbMainProperties(leave, new DbTreeLeave());
            result.Guid = Guid.Empty;
            return result;
        }
        internal static DbAttribute BusinessToDbAttribute(Entities.MainEntities.Attribute attribute)
        {
            var result = (DbAttribute)BusinessToDbMainProperties(attribute, new DbAttribute());
            return result;
        }
        #endregion
    }
}
