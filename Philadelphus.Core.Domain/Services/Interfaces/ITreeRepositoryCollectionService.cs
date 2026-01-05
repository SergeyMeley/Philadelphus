using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

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
