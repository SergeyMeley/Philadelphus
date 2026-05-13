using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    /// <summary>
    /// Задает контракт для работы с коллекции репозиториев Чубушника.
    /// </summary>
    public interface IPhiladelphusRepositoryCollectionService
    {
        #region [ Props ]

        /// <summary>
        /// Коллекция (словарь) репозиториев
        /// </summary>
        public static Dictionary<Guid, PhiladelphusRepositoryModel> DataPhiladelphusRepositories { get; }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить коллекцию элементов коллекции заголовков репозиториев Чубушника.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public IEnumerable<PhiladelphusRepositoryHeaderModel> GetPhiladelphusRepositoryHeadersCollection();

        /// <summary>
        /// Получить коллекцию репозиториев
        /// </summary>
        /// <param name="dataStorages">Хранилища данных</param>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns>Результат выполнения операции.</returns>
        public IEnumerable<PhiladelphusRepositoryModel> GetPhiladelphusRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null);

        /// <summary>
        /// Принудительная загрузка коллекции репозиториев из хранилищ
        /// </summary>
        /// <param name="dataStorages">Хранилище данных</param>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns>Результат выполнения операции.</returns>
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
        /// <param name="dataStorage">Хранилище данных.</param>
        /// <returns>Репозиторий</returns>
        /// <param name="needAutoName">Признак необходимости автоматической генерации наименования.</param>
        public PhiladelphusRepositoryModel CreateNewPhiladelphusRepository(IDataStorageModel dataStorage, bool needAutoName = true);

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
