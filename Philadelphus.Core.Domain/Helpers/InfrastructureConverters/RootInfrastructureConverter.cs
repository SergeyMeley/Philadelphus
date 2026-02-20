using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers;

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
            result.OwnDataStorageUuid = businessEntity.DataStorage.Uuid;
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
        public static TreeRootModel ToModel(this TreeRoot dbEntity, IEnumerable<IDataStorageModel> dataStorages, IEnumerable<PhiladelphusRepositoryModel> repositories)
        {
            if (dbEntity == null)
                return null;
            var dataStorage = dataStorages.FirstOrDefault(x => x.Uuid == dbEntity.OwnDataStorageUuid);
            var repository = repositories.FirstOrDefault(x => x.ContentShrub.ContentTreesUuids.Any(g => g == dbEntity.Uuid));
            var tree = new WorkingTreeModel(dbEntity.Uuid, dataStorage, dbEntity, repository.ContentShrub);
            var root = new TreeRootModel(dbEntity.Uuid, tree, dbEntity);
            root = (TreeRootModel)dbEntity.ToModelGeneralProperties(root);
            tree.ContentRoot = root;
            root = tree.ContentRoot;
            return root;
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="dataStorages">Коллекция доступных хранилищ данных</param>
        /// <param name="PhiladelphusRepositories">Коллекция доступных репозиториев Чубушника</param>
        /// <returns></returns>
        public static List<TreeRootModel> ToModelCollection(this IEnumerable<TreeRoot> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages, IEnumerable<PhiladelphusRepositoryModel> PhiladelphusRepositories)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeRootModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                result.Add(dbEntity.ToModel(dataStorages, PhiladelphusRepositories));
            }
            return result;
        }
    }
}
