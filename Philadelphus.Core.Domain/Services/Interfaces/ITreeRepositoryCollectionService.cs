using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface ITreeRepositoryCollectionService
    {
        #region [ Props ]

        /// <summary>
        /// Коллекция (словарь) репозиториев
        /// </summary>
        public static Dictionary<Guid, TreeRepositoryModel> DataTreeRepositories { get; }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить репозиторий из коллекции по уникальному идлентификатору
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <returns></returns>
        public TreeRepository GetTreeRepositoryFromCollection(Guid uuid);

        /// <summary>
        /// Получить коллекцию репозиториев из коллекции по уникальным идлентификаторам
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns></returns>
        public List<TreeRepository> GetTreeRepositoryFromCollection(IEnumerable<Guid> uuids);

        /// <summary>
        /// Получить репозиторий из коллекции по уникальному идлентификатору
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <returns></returns>
        public TreeRepositoryModel GetTreeRepositoryModelFromCollection(Guid uuid);

        /// <summary>
        /// Получить коллекцию репозиториев из коллекции по уникальным идлентификаторам
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns></returns>
        public List<TreeRepositoryModel> GetTreeRepositoryModelFromCollection(IEnumerable<Guid> uuids);

        /// <summary>
        /// Принудительная загрузка коллекции заголовков репозиториев из хранилищ
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeRepositoryHeaderModel> GetTreeRepositoryHeadersCollection();

        /// <summary>
        /// Получить коллекцию репозиториев
        /// </summary>
        /// <param name="dataStorages">Хранилища данных</param>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns></returns>
        public IEnumerable<TreeRepositoryModel> GetTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null);

        /// <summary>
        /// Принудительная загрузка коллекции репозиториев из хранилищ
        /// </summary>
        /// <param name="dataStorages">Хранилище данных</param>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns></returns>
        public IEnumerable<TreeRepositoryModel> ForceLoadTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null);

        #endregion

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий без содержимого и участников)
        /// </summary>
        /// <param name="treeRepository">Репозиторий</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(TreeRepositoryModel treeRepository);

        /// <summary>
        /// Сохранить изменения (заголовок репозитория без содержимого и участников)
        /// </summary>
        /// <param name="treeRepositoryHeader">Заголовок репозитория</param>
        /// <param name="dataStorageModel">Хранилище данных</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(TreeRepositoryHeaderModel treeRepositoryHeader, IDataStorageModel dataStorageModel);

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать новый репозиторий
        /// </summary>
        /// <param name="dataStorage"></param>
        /// <returns>Репозиторий</returns>
        public TreeRepositoryModel CreateNewTreeRepository(IDataStorageModel dataStorage);

        /// <summary>
        /// Создать заголовок репозитория из репозитория
        /// </summary>
        /// <param name="treeRepositoryModel">Репозиторий</param>
        /// <returns>Заголовок репозитория</returns>
        public TreeRepositoryHeaderModel CreateTreeRepositoryHeaderFromTreeRepository(TreeRepositoryModel treeRepositoryModel);

        /// <summary>
        /// Добавить существующий репозиторий
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Коллекция репозиториев</returns>
        public IEnumerable<TreeRepositoryModel> AddExistTreeRepository(DirectoryInfo path);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

    }
}
