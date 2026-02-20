using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    internal static class NodeInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static TreeNode ToDbEntity(this TreeNodeModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeNode)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            result.ParentUuid = businessEntity.Parent.Uuid;
            result.ParentTreeRootUuid = businessEntity.OwningWorkingTree.Uuid;     //TODO: ВРЕМЕННО
            //result.ParentRoot = (TreeRoot)businessEntity.ParentRoot.DbEntity;
            //result.ParentRoot = (TreeRoot)PhiladelphusRepositoryService.GetEntityFromCollection(businessEntity.ParentRoot.Uuid);
            if (businessEntity is SystemBaseTreeNodeModel st)
            {
                result.SystemBaseTypeId = (int)st.SystemBaseType;
            }
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
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

        /// <summary>
        /// Конвертировать сущность БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="parent">Родитель</param>
        /// <returns></returns>
        public static TreeNodeModel ToModel(this TreeNode dbEntity, IParentModel parent)
        {
            if (dbEntity == null)
                return null;

            WorkingTreeModel owner = null;
            if (parent is TreeNodeModel node)
                owner = node.OwningWorkingTree;
            if (parent is TreeRootModel root)
                owner = root.OwningWorkingTree;
            var result = new TreeNodeModel(dbEntity.Uuid, parent, owner, dbEntity);
            result = (TreeNodeModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }

        /// <summary>
        /// Конвертировать сущность БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="parent">Родитель</param>
        /// <returns></returns>
        public static SystemBaseTreeNodeModel ToBaseModel(this TreeNode dbEntity, IParentModel parent)
        {
            if (dbEntity == null)
                return null;

            WorkingTreeModel owner = null;
            if (parent is TreeNodeModel node)
                owner = node.OwningWorkingTree;
            if (parent is TreeRootModel root)
                owner = root.OwningWorkingTree;
            var result = new SystemBaseTreeNodeModel(dbEntity.Uuid, parent, owner);
            result = (SystemBaseTreeNodeModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="parents">Коллекция доступных ролдителей</param>
        /// <returns></returns>
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
                    if (dbEntity.SystemBaseTypeId != 0)
                    {
                        result.Add(dbEntity.ToBaseModel(parent));
                    }
                    else
                    {
                        result.Add(dbEntity.ToModel(parent));
                    }
                }
            }
            return result;
        }
    }
}
