using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    /// <summary>
    /// Сервис работы с хранилищами данных
    /// </summary>
    public interface IDataStoragesService
    {
        #region [ Props ]

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить коллекцию хранилищ данных
        /// </summary>
        /// <param name="connectionStrings">Строки подключения</param>
        /// <returns></returns>
        public IEnumerable<IDataStorageModel> GetStoragesModels(ConnectionStringsCollection connectionStrings);

        #endregion

        #region [ Save ]

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать основное хранилище данных
        /// </summary>
        /// <param name="storagesConfigFullPath">Путь к настроечному файлу хранилищ данных</param>
        /// <param name="repositoryHeadersConfigFullPath">Путь к настроечному файлу запусков репозиториев</param>
        /// <returns></returns>
        public IDataStorageModel CreateMainDataStorageModel(FileInfo storagesConfigFullPath, FileInfo repositoryHeadersConfigFullPath);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

    }
}
