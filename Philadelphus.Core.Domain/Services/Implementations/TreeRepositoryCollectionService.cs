using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Сервис работы с коллекцией репозиториев и хранилищами данных
    /// </summary>
    public class TreeRepositoryCollectionService : ITreeRepositoryCollectionService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger<TreeRepositoryCollectionService> _logger;
        private readonly INotificationService _notificationService;
        private readonly ITreeRepositoryService _treeRepositoryService;

        private static Dictionary<Guid, TreeRepositoryModel> _dataTreeRepositories = new Dictionary<Guid, TreeRepositoryModel>();
        /// <summary>
        /// Коллекция репозиториев
        /// </summary>
        public static Dictionary<Guid, TreeRepositoryModel> DataTreeRepositories { get => _dataTreeRepositories; private set => _dataTreeRepositories = value; }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Серсис работы с коллекцией репозиториев и хранилищами данных
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Сервис логгирования</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        /// <param name="treeRepositoryService">Сервис для работы с репозиторием и его элементами</param>
        public TreeRepositoryCollectionService(
            IMapper mapper,
            ILogger<TreeRepositoryCollectionService> logger,
            INotificationService notificationService,
            ITreeRepositoryService treeRepositoryService)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _treeRepositoryService = treeRepositoryService;

            _logger.LogInformation("TreeRepositoryCollectionService инициализирован.");
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получение репозитория по его UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns>Репозиторий</returns>
        public TreeRepository GetTreeRepositoryFromCollection(Guid uuid)
        {
            return GetTreeRepositoryModelFromCollection(uuid).ToDbEntity();
        }
        /// <summary>
        /// Получение коллекции репозиториев по их UUID
        /// </summary>
        /// <param name="uuids">UUIDs</param>
        /// <returns>Коллекция репозиториев</returns>
        public List<TreeRepository> GetTreeRepositoryFromCollection(IEnumerable<Guid> uuids)
        {
            return GetTreeRepositoryModelFromCollection(uuids).ToDbEntityCollection();
        }
        /// <summary>
        /// Получение репозитория (модель) по его UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns>Репозиторий (модель)</returns>
        public TreeRepositoryModel GetTreeRepositoryModelFromCollection(Guid uuid)
        {
            return _dataTreeRepositories[uuid];
        }
        /// <summary>
        /// Получение коллекции репозиториев (модели) по их UUID
        /// </summary>
        /// <param name="uuids">UUIDs</param>
        /// <returns>Коллекция репозиториев (модели)</returns>
        public List<TreeRepositoryModel> GetTreeRepositoryModelFromCollection(IEnumerable<Guid> uuids)
        {
            var result = new List<TreeRepositoryModel>();
            foreach (var uuid in uuids)
            {
                if (_dataTreeRepositories.TryGetValue(uuid, out var model))
                {
                    result.Add(model);
                }
            }
            return result;
        }

        /// <summary>
        /// Загрузка коллекции заголовков репозиториев (избранные или последние запускаемые) из настроечного файла
        /// </summary>
        /// <returns>Коллекция заголовков репозиториев (модели)</returns>
        public IEnumerable<TreeRepositoryHeaderModel> ForceLoadTreeRepositoryHeadersCollection(IDataStorageModel dataStorageModel)
        {
            if (dataStorageModel == null)
                return null;
            return dataStorageModel.TreeRepositoryHeadersCollectionInfrastructureRepository.SelectRepositoryCollection().ToModelCollection();
        }
        /// <summary>
        /// Получение коллекции репозиториев (модели) по их UUID из заданной коллекции хранилищ
        /// </summary>
        /// <param name="dataStorages">Коллекция хранилищ</param>
        /// <param name="uuids">UUIDs</param>
        /// <returns>Коллекция репозиториев (модели)</returns>
        public IEnumerable<TreeRepositoryModel> GetTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null)
        {
            IEnumerable<TreeRepositoryModel> result = DataTreeRepositories.Values;
            if (result == null || result?.Count() == 0)
            {
                result = ForceLoadTreeRepositoriesCollection(dataStorages, uuids);
            }
            return result;
        }
        /// <summary>
        /// Загрузка коллекции репозиториев (модели) по их UUID из заданной коллекции хранилищ
        /// </summary>
        /// <param name="dataStorages"></param>
        /// <param name="uuids"></param>
        /// <returns></returns>
        public IEnumerable<TreeRepositoryModel> ForceLoadTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null)
        {
            if (dataStorages == null)
                return null;
            var result = new List<TreeRepositoryModel>();
            foreach (var dataStorage in dataStorages.Where(x => x.TreeRepositoriesInfrastructureRepository != null))
            {
                var infrastructure = dataStorage.TreeRepositoriesInfrastructureRepository;
                if (infrastructure.GetType().IsAssignableTo(typeof(ITreeRepositoriesInfrastructureRepository))
                    && dataStorage.IsAvailable)
                {
                    IEnumerable<TreeRepository> dbRepositories = null;
                    if (uuids == null)
                        dbRepositories = infrastructure.SelectRepositories();
                    else
                        dbRepositories = infrastructure.SelectRepositories(uuids);
                    //var repositories = _mapper.Map<List<TreeRepositoryModel>>(dbRepositories);
                    var repositories = dbRepositories?.ToModelCollection(dataStorages);
                    if (repositories != null)
                    {
                        for (int i = 0; i < repositories.Count; i++)
                        {
                            repositories[i].State = State.SavedOrLoaded;
                        }
                        result.AddRange(repositories);
                    }
                }
            }
            return result;
        }

        #endregion

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        /// <param name="treeRepository">Репозиторий для сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(TreeRepositoryModel treeRepository)
        {
            long result = 0;
            result = _treeRepositoryService.SaveChanges(treeRepository);
            return result;
        }
        /// <summary>
        /// Сохранить изменения
        /// </summary>
        /// <param name="treeRepositoryHeader">Заголовок репозитория для сохранения</param>
        /// <param name="dataStorageModel">Хранилище данных</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(TreeRepositoryHeaderModel treeRepositoryHeader, IDataStorageModel dataStorageModel)
        {
            if (dataStorageModel == null)
                return -1;
            long result = 0;
            var dbEntity = treeRepositoryHeader.ToDbEntity();
            result += dataStorageModel.TreeRepositoryHeadersCollectionInfrastructureRepository.UpdateRepository(dbEntity);
            return result;
        }

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать новый репозиторий
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <returns>Репозиторий</returns>
        public TreeRepositoryModel CreateNewTreeRepository(IDataStorageModel dataStorage)
        {
            var result = new TreeRepositoryModel(Guid.NewGuid(), dataStorage, new TreeRepository());
            result.OwnDataStorage.TreeRepositoriesInfrastructureRepository.InsertRepository(result.ToDbEntity());
            return result;
        }
        //public bool TryCreateTreeRepositoryFromHeader(ITreeRepositoryHeaderModel header, out TreeRepositoryModel outTreeRepository)
        //{
        //    var dataStorage = DataStorageModels.FirstOrDefault(x => x.Value.Uuid == header.OwnDataStorageUuid).Value;
        //    var dbTreeRepository = DataTreeRepositories.FirstOrDefault(x => x.Value.Uuid == header.Uuid).Value.DbEntity;
        //    if (dataStorage != null && dbTreeRepository != null)
        //    {
        //        var result = new TreeRepositoryModel(header.Uuid, dataStorage, dbTreeRepository);
        //        outTreeRepository = result;
        //        return true;

        //    }
        //    //((ITreeRepositoriesInfrastructureRepository)result.OwnDataStorage).InsertRepository(result.ToDbEntity());
        //    outTreeRepository = null;
        //    return false;
        //}

        /// <summary>
        /// Создать заголовок репозитория из репозитория
        /// </summary>
        /// <param name="treeRepositoryModel">Репозиторий</param>
        /// <returns>Заголовок репозитория</returns>
        public TreeRepositoryHeaderModel CreateTreeRepositoryHeaderFromTreeRepository(TreeRepositoryModel treeRepositoryModel)
        {
            var result = new TreeRepositoryHeaderModel();
            result.Uuid = treeRepositoryModel.Uuid;
            result.Name = treeRepositoryModel.Name;
            result.Description = treeRepositoryModel.Description;
            result.OwnDataStorageName = treeRepositoryModel.OwnDataStorageName;
            result.OwnDataStorageUuid = treeRepositoryModel.OwnDataStorageUuid;
            result.IsFavorite = treeRepositoryModel.IsFavorite;
            result.State = treeRepositoryModel.State;
            return result;
        }
        /// <summary>
        /// Добавить коллекцию существующих репозиториев
        /// </summary>
        /// <param name="path">Путь к репозиторию</param>
        /// <returns>Коллекция репозиториев</returns>
        public IEnumerable<TreeRepositoryModel> AddExistTreeRepository(DirectoryInfo path)
        {
            var result = new List<TreeRepositoryModel>();
            return result;
        }

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

        #region [ Temp ]

        #endregion
    }
}
