using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Factories;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal static class RootInfrastructureConverter
    {
        public static TreeRoot ToDbEntity(this TreeRootModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeRoot)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            result.OwnDataStorageGuid = businessEntity.OwnDataStorage.Guid;
            result.DataStoragesGuids = businessEntity.DataStorages.Select(x => x.Guid).ToArray();
            return result;
        }
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
        public static TreeRootModel ToModel(this TreeRoot dbEntity, IEnumerable<IDataStorageModel> dataStorages, IEnumerable<TreeRepositoryModel> treeRepositories)
        {
            if (dbEntity == null)
                return null;
            var dataStorage = dataStorages.FirstOrDefault(x => x.Guid == dbEntity.OwnDataStorageGuid);
            var treeRepository = treeRepositories.FirstOrDefault(x => x.ChildsGuids.Any(g => g == dbEntity.Guid));
            var result = new TreeRootModel(dbEntity.Guid, treeRepository, dataStorage, dbEntity);
            result = (TreeRootModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }
        public static List<TreeRootModel> ToModelCollection(this IEnumerable<TreeRoot> dbEntityCollection, IEnumerable<IDataStorageModel> dataStorages, IEnumerable<TreeRepositoryModel> treeRepositories)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeRootModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                result.Add(dbEntity.ToModel(dataStorages, treeRepositories));
            }
            return result;
        }
    }
}
