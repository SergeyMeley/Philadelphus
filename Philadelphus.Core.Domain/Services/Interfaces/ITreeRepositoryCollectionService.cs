using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface IPhiladelphusRepositoryCollectionService
    {
        #region [ Props ]

        /// <summary>
        /// Коллекция (словарь) репозиториев
        /// </summary>
        public static Dictionary<Guid, PhiladelphusRepositoryModel> DataPhiladelphusRepositories { get; }

        #endregion

        #region [ Get + Load ]

        public IEnumerable<PhiladelphusRepositoryHeaderModel> GetPhiladelphusRepositoryHeadersCollection();

        /// <summary>
        /// Получить коллекцию репозиториев
        /// </summary>
        /// <param name="dataStorages">Хранилища данных</param>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns></returns>
        public IEnumerable<PhiladelphusRepositoryModel> GetPhiladelphusRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null);

        /// <summary>
        /// Принудительная загрузка коллекции репозиториев из хранилищ
        /// </summary>
        /// <param name="dataStorages">Хранилище данных</param>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns></returns>
        public IEnumerable<PhiladelphusRepositoryModel> ForceLoadPhiladelphusRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null);

        #endregion

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий без содержимого и участников)
        /// </summary>
        /// <param name="PhiladelphusRepository">Репозиторий</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(ref PhiladelphusRepositoryModel PhiladelphusRepository);

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать новый репозиторий
        /// </summary>
        /// <param name="dataStorage"></param>
        /// <returns>Репозиторий</returns>
        public PhiladelphusRepositoryModel CreateNewPhiladelphusRepository(IDataStorageModel dataStorage);

        /// <summary>
        /// Создать заголовок репозитория из репозитория
        /// </summary>
        /// <param name="PhiladelphusRepositoryModel">Репозиторий</param>
        /// <returns>Заголовок репозитория</returns>
        public PhiladelphusRepositoryHeaderModel CreatePhiladelphusRepositoryHeaderFromPhiladelphusRepository(PhiladelphusRepositoryModel PhiladelphusRepositoryModel);

        /// <summary>
        /// Добавить существующий репозиторий
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Коллекция репозиториев</returns>
        public IEnumerable<PhiladelphusRepositoryModel> AddExistPhiladelphusRepository(DirectoryInfo path);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

    }
}
