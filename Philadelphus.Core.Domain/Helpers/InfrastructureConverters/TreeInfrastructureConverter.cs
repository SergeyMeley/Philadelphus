using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    internal static class TreeInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static WorkingTree ToDbEntity(this WorkingTreeModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (WorkingTree)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            result.OwnDataStorageUuid = businessEntity.DataStorage.Uuid;
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
        public static List<WorkingTree> ToDbEntityCollection(this IEnumerable<WorkingTreeModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<WorkingTree>();
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
        /// <param name="repository">Доступные репозитории Чубушника</param>
        /// <returns></returns>
        public static WorkingTreeModel ToModel(this WorkingTree dbEntity, IDataStorageModel dataStorage, PhiladelphusRepositoryModel repository)
        {
            if (dbEntity == null)
                return null;
            if (dataStorage == null || repository == null)
                throw new ArgumentNullException();
            var result = new WorkingTreeModel(dbEntity.Uuid, dataStorage, dbEntity, repository.ContentShrub);
            result = (WorkingTreeModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="dataStorages">Коллекция доступных хранилищ данных</param>
        /// <param name="PhiladelphusRepositories">Коллекция доступных репозиториев Чубушника</param>
        /// <returns></returns>
        public static List<WorkingTreeModel> ToModelCollection(this IEnumerable<WorkingTree> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages, PhiladelphusRepositoryModel repository)
        {
            if (dbEntityCollection == null)
                return null;
            if (dbEntityCollection.Count() == 0)
                return new List<WorkingTreeModel>();
            if (dataStorages == null || dataStorages.Count() == 0 || repository == null)
                throw new ArgumentNullException();
            var result = new List<WorkingTreeModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var dataStorage = dataStorages.SingleOrDefault(x => x.Uuid == dbEntity.OwnDataStorageUuid);
                result.Add(dbEntity.ToModel(dataStorage, repository));
            }
            return result;
        }
    }

}
