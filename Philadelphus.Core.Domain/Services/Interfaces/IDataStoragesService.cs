using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface IDataStoragesService
    {
        #region [ Props ]

        #endregion

        #region [ Get + Load ]

        public IEnumerable<IDataStorageModel> GetStoragesModels(ConnectionStringsCollection connectionStrings);

        #endregion

        #region [ Save ]

        #endregion

        #region [ Create + Add ]

        public IDataStorageModel CreateMainDataStorageModel(FileInfo storagesConfigFullPath, FileInfo repositoryHeadersConfigFullPath);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

    }
}
