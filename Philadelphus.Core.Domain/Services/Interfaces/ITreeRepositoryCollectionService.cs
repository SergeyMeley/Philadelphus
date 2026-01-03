using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using Philadelphus.Infrastructure.Persistence.Json.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphus.Core.Domain.Config;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface ITreeRepositoryCollectionService
    {
        #region [ Props ]

        public static Dictionary<Guid, TreeRepositoryModel> DataTreeRepositories { get; }

        #endregion

        #region [ Get + Load ]

        public TreeRepository GetTreeRepositoryFromCollection(Guid uuid);
        public List<TreeRepository> GetTreeRepositoryFromCollection(IEnumerable<Guid> uuids);
        public TreeRepositoryModel GetTreeRepositoryModelFromCollection(Guid uuid);
        public List<TreeRepositoryModel> GetTreeRepositoryModelFromCollection(IEnumerable<Guid> uuids);
        public IEnumerable<TreeRepositoryHeaderModel> ForceLoadTreeRepositoryHeadersCollection(IDataStorageModel dataStorageModel);
        public IEnumerable<TreeRepositoryModel> GetTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null);
        public IEnumerable<TreeRepositoryModel> ForceLoadTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null);

        #endregion

        #region [ Save ]

        public long SaveChanges(TreeRepositoryModel treeRepository);
        public long SaveChanges(TreeRepositoryHeaderModel treeRepositoryHeader, IDataStorageModel dataStorageModel);

        #endregion

        #region [ Create + Add ]

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
