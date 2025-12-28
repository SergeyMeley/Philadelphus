using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    internal static class InfrastructureConverterBase
    {
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
        public static IMainEntity ToDbEntityGeneralProperties(this MainEntityBaseModel businessEntity, IMainEntity dbEntity)
        {
            if (businessEntity == null)
                return null;
            //dbEntity.ParentGuid = businessEntity.Parent.ToString();
            //dbEntity.DirectoryPath = businessEntity.DirectoryPath;
            //dbEntity.DirectoryFullPath = businessEntity.DirectoryFullPath;
            //dbEntity.ConfigPath = businessEntity.ConfigPath;
            dbEntity.Guid = businessEntity.Guid;
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
        public static List<IMainEntity> ToDbEntityCollection(this IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            List<IMainEntity> result = null;
            foreach (var item in businessEntityCollection)
            {
                result.Add(item.ToDbEntity());
            }
            return result;
        }
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
        public static List<IMainEntityModel> ToModelCollection(this IEnumerable<IMainEntity> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages)
        {
            List<IMainEntityModel> result = null;
            foreach (var item in dbEntityCollection)
            {
                result.Add(item.ToModel(dataStorages));
            }
            return result;
        }
    }
}
