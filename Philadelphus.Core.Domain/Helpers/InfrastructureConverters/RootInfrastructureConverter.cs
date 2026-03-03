using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    internal static class RootInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static TreeRoot ToDbEntity(this TreeRootModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeRoot)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            result.OwningWorkingTreeUuid = businessEntity.OwningWorkingTree.Uuid;
            result.OwningWorkingTree = businessEntity.OwningWorkingTree.ToDbEntity();
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
        public static List<TreeRoot> ToDbEntityCollection(this IEnumerable<TreeRootModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeRoot>();
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
        /// <param name="dataStorages">Доступные хранилища данных</param>
        /// <param name="repositories">Доступные репозитории Чубушника</param>
        /// <returns></returns>
        public static TreeRootModel ToModel(this TreeRoot dbEntity, WorkingTreeModel workingTree)
        {
            if (dbEntity == null)
                return null;
            if (workingTree == null)
                throw new ArgumentNullException();
            var root = new TreeRootModel(dbEntity.Uuid, workingTree, dbEntity);
            root = (TreeRootModel)dbEntity.ToModelGeneralProperties(root);
            workingTree.ContentRoot = root;
            return root;
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="workingTrees">Коллекция рабочих деревьев</param>
        /// <returns></returns>
        public static List<TreeRootModel> ToModelCollection(this IEnumerable<TreeRoot> dbEntityCollection, IEnumerable<WorkingTreeModel> workingTrees)
        {
            if (dbEntityCollection == null)
                return null;
            if (dbEntityCollection.Count() == 0)
                return new List<TreeRootModel>();
            if (workingTrees == null || workingTrees.Count() == 0)
                throw new ArgumentNullException();
            var result = new List<TreeRootModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var tree = workingTrees.FirstOrDefault(x => x.Uuid == dbEntity.OwningWorkingTreeUuid);
                result.Add(dbEntity.ToModel(tree));
            }
            return result;
        }
    }
}
