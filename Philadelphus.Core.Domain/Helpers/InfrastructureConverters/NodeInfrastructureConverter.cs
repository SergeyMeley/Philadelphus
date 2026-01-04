using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    internal static class NodeInfrastructureConverter
    {
        public static TreeNode ToDbEntity(this TreeNodeModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeNode)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            result.ParentUuid = businessEntity.Parent.Uuid;
            result.ParentTreeRootUuid = businessEntity.ParentRoot.Uuid;     //TODO: ВРЕМЕННО
            //result.ParentRoot = (TreeRoot)businessEntity.ParentRoot.DbEntity;
            //result.ParentRoot = (TreeRoot)TreeRepositoryService.GetEntityFromCollection(businessEntity.ParentRoot.Uuid);
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
            var result = new TreeNodeModel(dbEntity.Uuid, parent, dbEntity);
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
                var parent = parents.FirstOrDefault(x => x.Uuid == dbEntity.ParentUuid);
                if (parent != null)
                {
                    result.Add(dbEntity.ToModel(parent));
                }
            }
            return result;
        }
    }
}
