using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Сервис работы с коллекцией репозиториев
    /// </summary>
    public class PhiladelphusRepositoryCollectionService : IPhiladelphusRepositoryCollectionService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger<PhiladelphusRepositoryCollectionService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IPhiladelphusRepositoryService _philadelphusRepositoryService;
        private readonly IOptions<ApplicationSettingsConfig> _applicationSettings;
        private readonly IOptions<PhiladelphusRepositoryHeadersCollectionConfig> _PhiladelphusRepositoryHeadersCollection;

        private static Dictionary<Guid, PhiladelphusRepositoryModel> _dataPhiladelphusRepositories = new Dictionary<Guid, PhiladelphusRepositoryModel>();

        /// <summary>
        /// Коллекция репозиториев
        /// </summary>
        public static Dictionary<Guid, PhiladelphusRepositoryModel> DataPhiladelphusRepositories { get => _dataPhiladelphusRepositories; private set => _dataPhiladelphusRepositories = value; }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Серсис работы с коллекцией репозиториев и хранилищами данных
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Логгер</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        /// <param name="PhiladelphusRepositoryService">Сервис работы с репозиторием и его элементами</param>
        /// <param name="applicationSettings"></param>
        /// <param name="PhiladelphusRepositoryHeadersCollection"></param>
        public PhiladelphusRepositoryCollectionService(
            IMapper mapper,
            ILogger<PhiladelphusRepositoryCollectionService> logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryService PhiladelphusRepositoryService,
            IOptions<ApplicationSettingsConfig> applicationSettings,
            IOptions<PhiladelphusRepositoryHeadersCollectionConfig> PhiladelphusRepositoryHeadersCollection)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _philadelphusRepositoryService = PhiladelphusRepositoryService;
            _applicationSettings = applicationSettings;
            _PhiladelphusRepositoryHeadersCollection = PhiladelphusRepositoryHeadersCollection;

            _logger.LogInformation("PhiladelphusRepositoryCollectionService инициализирован.");
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получение репозитория по его уникальному идентификатору
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <returns>Репозиторий</returns>
        public PhiladelphusRepository GetPhiladelphusRepositoryFromCollection(Guid uuid)
        {
            return GetPhiladelphusRepositoryModelFromCollection(uuid).ToDbEntity();
        }
        /// <summary>
        /// Получение коллекции репозиториев по их уникальным идентификаторам
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns>Коллекция репозиториев</returns>
        public List<PhiladelphusRepository> GetPhiladelphusRepositoryFromCollection(IEnumerable<Guid> uuids)
        {
            return GetPhiladelphusRepositoryModelFromCollection(uuids).ToDbEntityCollection();
        }
        /// <summary>
        /// Получение репозитория (модель) по его уникальному идентификатору
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <returns>Репозиторий (модель)</returns>
        public PhiladelphusRepositoryModel GetPhiladelphusRepositoryModelFromCollection(Guid uuid)
        {
            return _dataPhiladelphusRepositories[uuid];
        }
        /// <summary>
        /// Получение коллекции репозиториев (модели) по их UUID
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы</param>
        /// <returns>Коллекция репозиториев (модели)</returns>
        public List<PhiladelphusRepositoryModel> GetPhiladelphusRepositoryModelFromCollection(IEnumerable<Guid> uuids)
        {
            var result = new List<PhiladelphusRepositoryModel>();
            foreach (var uuid in uuids)
            {
                if (_dataPhiladelphusRepositories.TryGetValue(uuid, out var model))
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
        public IEnumerable<PhiladelphusRepositoryHeaderModel> GetPhiladelphusRepositoryHeadersCollection()
        {
            var dbEntities = _PhiladelphusRepositoryHeadersCollection.Value.PhiladelphusRepositoryHeaders;
            var result = dbEntities.ToModelCollection();
            return result;
        }
        /// <summary>
        /// Получение коллекции репозиториев (модели) по их UUID из заданной коллекции хранилищ
        /// </summary>
        /// <param name="dataStorages">Коллекция хранилищ</param>
        /// <param name="uuids">UUIDs</param>
        /// <returns>Коллекция репозиториев (модели)</returns>
        public IEnumerable<PhiladelphusRepositoryModel> GetPhiladelphusRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null)
        {
            IEnumerable<PhiladelphusRepositoryModel> result = DataPhiladelphusRepositories.Values;
            if (result == null || result?.Count() == 0)
            {
                result = ForceLoadPhiladelphusRepositoriesCollection(dataStorages, uuids);
            }
            return result;
        }
        /// <summary>
        /// Загрузка коллекции репозиториев (модели) по их UUID из заданной коллекции хранилищ
        /// </summary>
        /// <param name="dataStorages"></param>
        /// <param name="uuids"></param>
        /// <returns></returns>
        public IEnumerable<PhiladelphusRepositoryModel> ForceLoadPhiladelphusRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] uuids = null)
        {
            if (dataStorages == null)
                return null;
            var result = new List<PhiladelphusRepositoryModel>();
            foreach (var dataStorage in dataStorages.Where(x => x.PhiladelphusRepositoriesInfrastructureRepository != null))
            {
                var infrastructure = dataStorage.PhiladelphusRepositoriesInfrastructureRepository;
                if (infrastructure.GetType().IsAssignableTo(typeof(IPhiladelphusRepositoriesInfrastructureRepository))
                    && dataStorage.IsAvailable)
                {
                    IEnumerable<PhiladelphusRepository> dbRepositories = null;
                    if (uuids == null)
                        dbRepositories = infrastructure.SelectRepositories();
                    else
                        dbRepositories = infrastructure.SelectRepositories(uuids);
                    //var repositories = _mapper.Map<List<PhiladelphusRepositoryModel>>(dbRepositories);
                    var repositories = dbRepositories?.ToModelCollection(dataStorages);
                    if (repositories != null)
                    {
                        for (int i = 0; i < repositories.Count; i++)
                        {
                            SetModelState(repositories[i], State.SavedOrLoaded);
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
        /// Сохранить изменения (репозиторий без содержимого и участников)
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(ref PhiladelphusRepositoryModel repository)
        {
            long result = 0;
            result = _philadelphusRepositoryService.SaveChanges(ref repository, SaveMode.OnlyHeader);
            return result;
        }

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать новый репозиторий
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <returns>Репозиторий</returns>
        public PhiladelphusRepositoryModel CreateNewPhiladelphusRepository(IDataStorageModel dataStorage)
        {
            var result = new PhiladelphusRepositoryModel(Guid.NewGuid(), dataStorage, new PhiladelphusRepository());
            result.ContentShrub.ContentTreesUuids.Add(WorkingTreeModel.SystemBaseGuid);
            return result;
        }

        /// <summary>
        /// Создать заголовок репозитория из репозитория
        /// </summary>
        /// <param name="philadelphusRepositoryModel">Репозиторий</param>
        /// <returns>Заголовок репозитория</returns>
        public PhiladelphusRepositoryHeaderModel CreatePhiladelphusRepositoryHeaderFromPhiladelphusRepository(PhiladelphusRepositoryModel philadelphusRepositoryModel)
        {
            var result = new PhiladelphusRepositoryHeaderModel(philadelphusRepositoryModel.Uuid);
            result.Name = philadelphusRepositoryModel.Name;
            result.Description = philadelphusRepositoryModel.Description;
            result.OwnDataStorageName = philadelphusRepositoryModel.OwnDataStorageName;
            result.OwnDataStorageUuid = philadelphusRepositoryModel.OwnDataStorageUuid;
            result.IsFavorite = philadelphusRepositoryModel.IsFavorite;
            result.State = philadelphusRepositoryModel.State;
            result.LastOpening = DateTime.UtcNow;
            return result;
        }
        /// <summary>
        /// Добавить коллекцию существующих репозиториев
        /// </summary>
        /// <param name="path">Путь к репозиторию</param>
        /// <returns>Коллекция репозиториев</returns>
        public IEnumerable<PhiladelphusRepositoryModel> AddExistPhiladelphusRepository(DirectoryInfo path)
        {
            var result = new List<PhiladelphusRepositoryModel>();
            return result;
        }

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

        #region [ Private methods ]

        /// <summary>
        /// Изменить статус элемента
        /// </summary>
        /// <param name="model">Элемент</param>
        /// <param name="newState">Новый статус</param>
        private void SetModelState(IMainEntityWritableModel model, State newState)
        {
            model.SetState(newState);
        }

        #endregion

        #region [ Temp ]

        #endregion
    }
}
