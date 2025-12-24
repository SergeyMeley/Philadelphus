using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Business.Factories;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal static class NodeInfrastructureConverter
    {
        public static TreeNode ToDbEntity(this TreeNodeModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeNode)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            result.ParentGuid = businessEntity.Parent.Guid;
            result.ParentTreeRootGuid = businessEntity.ParentRoot.Guid;     //TODO: ВРЕМЕННО
            //result.ParentRoot = (TreeRoot)businessEntity.ParentRoot.DbEntity;
            //result.ParentRoot = (TreeRoot)TreeRepositoryService.GetEntityFromCollection(businessEntity.ParentRoot.Guid);
            return result;
        }
        public static List<TreeNode> ToDbEntityCollection(this IEnumerable<TreeNodeModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeNode>();
            foreach (var businessEntity in businessEntityCollection)
            {
                result.Add(businessEntity.ToDbEntity());
            }
            return result;
        }
        public static TreeNodeModel ToModel(this TreeNode dbEntity, IParentModel parent)
        {
            if (dbEntity == null)
                return null;
            var result = new TreeNodeModel(dbEntity.Guid, parent, dbEntity);
            result = (TreeNodeModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }
        public static List<TreeNodeModel> ToModelCollection(this IEnumerable<TreeNode> dbEntityCollection, IEnumerable<IParentModel> parents)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeNodeModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var parent = parents.FirstOrDefault(x => x.Guid == dbEntity.ParentGuid);
                if (parent != null)
                {
                    result.Add(dbEntity.ToModel(parent));
                }
            }
            return result;
        }
    }
}
