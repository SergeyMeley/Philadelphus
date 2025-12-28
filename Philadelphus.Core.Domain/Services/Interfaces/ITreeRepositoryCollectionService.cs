using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.JsonRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface ITreeRepositoryCollectionService
    {
        #region [ Props ]

        public static Dictionary<Guid, IDataStorageModel> DataStorages { get; }
        public static Dictionary<Guid, TreeRepositoryModel> DataTreeRepositories { get; }

        #endregion

        #region [ Get + Load ]

        public TreeRepository GetTreeRepositoryFromCollection(Guid guid);
        public List<TreeRepository> GetTreeRepositoryFromCollection(IEnumerable<Guid> guids);
        public TreeRepositoryModel GetTreeRepositoryModelFromCollection(Guid guid);
        public List<TreeRepositoryModel> GetTreeRepositoryModelFromCollection(IEnumerable<Guid> guids);
        public IDataStorageModel GetStorageModelFromCollection(Guid guid);
        public List<IDataStorageModel> GetStorageModelFromCollection(IEnumerable<Guid> guids);
        public IEnumerable<TreeRepositoryHeaderModel> ForceLoadTreeRepositoryHeadersCollection(IDataStorageModel dataStorageModel);
        public IEnumerable<TreeRepositoryModel> GetTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] guids = null);
        public IEnumerable<TreeRepositoryModel> ForceLoadTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] guids = null);

        #endregion

        #region [ Save ]

        public long SaveChanges(TreeRepositoryModel treeRepository);
        public long SaveChanges(TreeRepositoryHeaderModel treeRepositoryHeader, IDataStorageModel dataStorageModel);

        #endregion

        #region [ Create + Add ]

        public IDataStorageModel CreateMainDataStorageModel(DirectoryInfo configsDirectory);
        public TreeRepositoryModel CreateNewTreeRepository(IDataStorageModel dataStorage);
        public TreeRepositoryHeaderModel CreateTreeRepositoryHeaderFromTreeRepository(TreeRepositoryModel treeRepositoryModel);
        public IEnumerable<TreeRepositoryModel> AddExistTreeRepository(DirectoryInfo path);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

    }
}
