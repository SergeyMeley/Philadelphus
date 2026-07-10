using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Identity.Services.Interfaces;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Json.Repositories;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Сервис работы с хранилищами данных
    /// </summary>
    public class DataStoragesService : IDataStoragesService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IOptions<ApplicationSettingsConfig> _applicationSettings;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollection;
        private readonly IOptions<DataStoragesCollectionConfig> _dataStoragesCollection;

        #endregion

        #region [ Construct ]


        /// <summary>
        /// Серсис работы с коллекцией репозиториев и хранилищами данных
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Сервис логгирования</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        /// <param name="applicationSettings">Настройки приложения.</param>
        /// <param name="connectionStringsCollection">Коллекция строк подключения.</param>
        /// <param name="dataStoragesCollection">Коллекция хранилищ данных.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public DataStoragesService(
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IUserService userService,
            IOptions<ApplicationSettingsConfig> applicationSettings,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollection,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollection)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(userService);
            ArgumentNullException.ThrowIfNull(applicationSettings);
            ArgumentNullException.ThrowIfNull(connectionStringsCollection);
            ArgumentNullException.ThrowIfNull(dataStoragesCollection);

            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _userService = userService;
            _applicationSettings = applicationSettings;
            _connectionStringsCollection = connectionStringsCollection;
            _dataStoragesCollection = dataStoragesCollection;

            Log.Information("DataStoragesService инициализирован.");
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить коллекцию хранилищ данных
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="getInfrastructureRepository">Функция получения инфраструктурного репозитория.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="InvalidOperationException">Если операция недопустима для текущего состояния объекта.</exception>
        public IEnumerable<IDataStorageModel> GetStoragesModels(
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository)
        {
            ArgumentNullException.ThrowIfNull(getInfrastructureRepository);

            // Валидация конфигурации
            var legacyConnectionStrings = _connectionStringsCollection.Value?.ConnectionStringsContainers
                ?? throw new InvalidOperationException(
                    "ConnectionStrings не инициализированы. Проверьте конфиг ConnectionStringsCollectionConfig");

            var dbEntities = _dataStoragesCollection.Value?.DataStorages
                ?? throw new InvalidOperationException(
                    "DataStorages не инициализированы. Проверьте конфиг DataStoragesCollectionConfig");

            foreach (var entity in dbEntities)
            {
                var connectionString = GetConnectionStringsContainer(
                    entity,
                    legacyConnectionStrings.SingleOrDefault(x => x.StorageUuid == entity.Uuid));

                if (connectionString == null)
                {
                    _logger.Error($"Не найдена строка подключения для хранилища {entity.Name}");
                    _notificationService.SendTextMessage<DataStoragesService>($"Не найдена строка подключения для хранилища {entity.Name}");
                    continue;
                }

                var infrastructureRepositories = new List<IInfrastructureRepository>();
                var isDisabled = entity.IsDisabled ?? false;
                if (isDisabled == false)
                {
                    if (entity.HasPhiladelphusRepositoriesInfrastructureRepository)
                        TryAddInfrastructureRepository(
                            infrastructureRepositories,
                            getInfrastructureRepository,
                            connectionString,
                            entity,
                            InfrastructureEntityGroups.PhiladelphusRepositories);
                    if (entity.HasShrubMembersInfrastructureRepository)
                        TryAddInfrastructureRepository(
                            infrastructureRepositories,
                            getInfrastructureRepository,
                            connectionString,
                            entity,
                            InfrastructureEntityGroups.ShrubMembers);
                    if (entity.HasReportsInfrastructureRepository)
                        TryAddInfrastructureRepository(
                            infrastructureRepositories,
                            getInfrastructureRepository,
                            connectionString,
                            entity,
                            InfrastructureEntityGroups.Reports);
                }

                var model = _mapper.MapDataStorage(entity, infrastructureRepositories, _logger);
                yield return model;
            }
        }

        private static ConnectionStringsContainer? GetConnectionStringsContainer(
            DataStorage dataStorage,
            ConnectionStringsContainer? legacyConnectionStrings)
        {
            if (dataStorage.ConnectionStrings != null && dataStorage.ConnectionStrings.Count > 0)
            {
                return new ConnectionStringsContainer
                {
                    StorageUuid = dataStorage.Uuid,
                    ProviderName = string.IsNullOrWhiteSpace(dataStorage.ProviderName)
                        ? legacyConnectionStrings?.ProviderName
                        : dataStorage.ProviderName,
                    InfrastructureType = dataStorage.InfrastructureType,
                    ConnectionStrings = dataStorage.ConnectionStrings
                };
            }

            return legacyConnectionStrings;
        }

        private void TryAddInfrastructureRepository(
            ICollection<IInfrastructureRepository> infrastructureRepositories,
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository,
            ConnectionStringsContainer connectionString,
            DataStorage entity,
            InfrastructureEntityGroups entityGroup)
        {
            if (HasConnectionString(connectionString, entityGroup) == false)
                return;

            try
            {
                var infrastructureRepository = getInfrastructureRepository(connectionString, entity.InfrastructureType, entityGroup);
                if (infrastructureRepository != null)
                {
                    infrastructureRepositories.Add(infrastructureRepository);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(
                    exception,
                    "Не удалось инициализировать инфраструктурный репозиторий {EntityGroup} для хранилища {DataStorageName} ({DataStorageUuid}).",
                    entityGroup,
                    entity.Name,
                    entity.Uuid);
                _notificationService.SendTextMessage<DataStoragesService>(
                    $"Не удалось инициализировать инфраструктурный репозиторий {entityGroup} для хранилища {entity.Name}");
            }
        }

        private static bool HasConnectionString(
            ConnectionStringsContainer connectionString,
            InfrastructureEntityGroups entityGroup)
        {
            return connectionString.ConnectionStrings != null
                && connectionString.ConnectionStrings.TryGetValue(entityGroup, out var value)
                && string.IsNullOrWhiteSpace(value) == false;
        }

        #endregion

        #region [ Save ]

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать основное хранилище данных
        /// </summary>
        /// <param name="basePath">Базовый путь.</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="InvalidOperationException">Если операция недопустима для текущего состояния объекта.</exception>
        public IDataStorageModel CreateMainDataStorageModel(DirectoryInfo basePath)
        {
            ArgumentNullException.ThrowIfNull(basePath);

            var settings = _applicationSettings.Value
                ?? throw new InvalidOperationException("ApplicationSettings не инициализированы");
            var mainDataStorage = settings.MainDataStorage
                ?? throw new InvalidOperationException("MainDataStorage не инициализирован");

            var path = mainDataStorage.FullName;
            var auditUserName = _userService.CurrentUser.UserName;

            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder()
                .SetGeneralParameters(
                    logger: _logger,
                    name: "Основное хранилище",
                    description: "Основное хранилище",
                    uuid: DataStorageModel.MainDataStorageUuid,
                    infrastructureType: InfrastructureTypes.SQLiteEf,
                    isDisabled: false)
            .SetRepository(new SqliteEfPhiladelphusRepositoriesInfrastructureRepository(_logger, $"Data Source={Path.Combine(path, "main-repositories-data-storage.db")}", auditUserName))
            .SetRepository(new SqliteEfShrubMembersInfrastructureRepository(_logger, $"Data Source={Path.Combine(path, "main-working-trees-data-storage.db")}", auditUserName))
            .SetRepository(new SqliteEfReportsInfrastructureRepository(_logger, $"Data Source={Path.Combine(path, "main-reports-data-storage.db")}", auditUserName));

            var mainDataStorageModel = dataStorageBuilder.Build();

            Log.Information("Базовое хранилище инициализировано.");

            return mainDataStorageModel;
        }

        /// <summary>
        /// /// <summary>
        /// Создать хранилище данных
        /// <param name="name">Наименование</param>
        /// <param name="desctiption">Описание</param>
        /// <param name="connectionString">Строка подключенияя</param>
        /// <param name="getInfrastructureRepository">Функция получения инфраструктурного репозитория.</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
        public IDataStorageModel CreateDataStorageModel(
            string name, 
            string desctiption, 
            ConnectionStringsContainer connectionString,
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(connectionString);
            ArgumentNullException.ThrowIfNull(getInfrastructureRepository);

            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder()
                .SetGeneralParameters(
                    logger: _logger,
                    name: name,
                    description: desctiption,
                    uuid: connectionString.StorageUuid,
                    infrastructureType: connectionString.InfrastructureType,
                    isDisabled: false,
                    providerName: connectionString.ProviderName,
                    connectionStrings: connectionString.ConnectionStrings);

            TrySetInfrastructureRepository(
                dataStorageBuilder,
                getInfrastructureRepository,
                connectionString,
                InfrastructureEntityGroups.PhiladelphusRepositories);
            TrySetInfrastructureRepository(
                dataStorageBuilder,
                getInfrastructureRepository,
                connectionString,
                InfrastructureEntityGroups.ShrubMembers);
            TrySetInfrastructureRepository(
                dataStorageBuilder,
                getInfrastructureRepository,
                connectionString,
                InfrastructureEntityGroups.Reports);

            var dataStorageModel = dataStorageBuilder.Build();

            Log.Information("Хранилище инициализировано.");

            return dataStorageModel;
        }

        private static void TrySetInfrastructureRepository(
            DataStorageBuilder dataStorageBuilder,
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository,
            ConnectionStringsContainer connectionString,
            InfrastructureEntityGroups entityGroup)
        {
            if (HasConnectionString(connectionString, entityGroup) == false)
                return;

            dataStorageBuilder.SetRepository(
                getInfrastructureRepository(connectionString, connectionString.InfrastructureType, entityGroup));
        }

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion
    }
}
