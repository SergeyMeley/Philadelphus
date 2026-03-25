using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
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
        /// <param name="applicationSettings"></param>
        /// <param name="connectionStringsCollection"></param>
        /// <param name="dataStoragesCollection"></param>
        public DataStoragesService(
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IOptions<ApplicationSettingsConfig> applicationSettings,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollection,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollection)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
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
        /// <returns></returns>
        public IEnumerable<IDataStorageModel> GetStoragesModels(
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository)
        {
            var connectionStrings = _connectionStringsCollection.Value.ConnectionStringsContainers;
            var dbEntities = _dataStoragesCollection.Value.DataStorages;
            var models = new List<IDataStorageModel>();

            foreach (var entity in dbEntities)
            {
                var connectionString = connectionStrings.SingleOrDefault(x => x.StorageUuid == entity.Uuid);

                if (connectionString == null)
                {
                    _logger.Error($"Не найдена строка подключения для хранилища {entity.Name}");
                    _notificationService.SendTextMessage<DataStoragesService>($"Не найдена строка подключения для хранилища {entity.Name}");
                }
                else
                {
                    var infrastructureRepositories = new List<IInfrastructureRepository>();
                    if (entity.IsHidden == false)
                    {
                        if (entity.HasPhiladelphusRepositoriesInfrastructureRepository)
                            infrastructureRepositories.Add(getInfrastructureRepository(connectionString, entity.InfrastructureType, InfrastructureEntityGroups.PhiladelphusRepositories));
                        if (entity.HasShrubMembersInfrastructureRepository)
                            infrastructureRepositories.Add(getInfrastructureRepository(connectionString, entity.InfrastructureType, InfrastructureEntityGroups.ShrubMembers));
                    }

                    var model = _mapper.MapDataStorage(entity, infrastructureRepositories, _logger);
                    yield return model;
                }
            }
        }

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
        public IDataStorageModel CreateMainDataStorageModel(DirectoryInfo basePath)
        {
            if (basePath == null)
                throw new ArgumentNullException($"{nameof(basePath)}");

            var path = _applicationSettings.Value.MainDataStorage.FullName;

            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder()
                .SetGeneralParameters(
                    logger: _logger,
                    name: "Основное хранилище",
                    description: "Основное хранилище",
                    uuid: Guid.Parse("00000000-0000-0000-0000-19201518a07e"),
                    infrastructureType: InfrastructureTypes.SQLiteEf,
                    isDisabled: false)
            .SetRepository(new SqliteEfPhiladelphusRepositoriesInfrastructureRepository(_logger, $"Data Source={Path.Combine(path, "main-repositories-data-storage.db")}"))
            .SetRepository(new SqliteEfShrubMembersInfrastructureRepository(_logger, $"Data Source={Path.Combine(path, "main-working-trees-data-storage.db")}"));

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
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IDataStorageModel CreateDataStorageModel(string name, string desctiption, ConnectionStringsContainer connectionString,
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository)
        {
            if (string.IsNullOrEmpty(name) || getInfrastructureRepository == null)
                throw new ArgumentNullException();

            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder()
                .SetGeneralParameters(
                    logger: _logger,
                    name: name,
                    description: desctiption,
                    uuid: connectionString.StorageUuid,
                    infrastructureType: connectionString.InfrastructureType,
                    isDisabled: false)
                .SetRepository(getInfrastructureRepository(connectionString, connectionString.InfrastructureType, InfrastructureEntityGroups.PhiladelphusRepositories))
                .SetRepository(getInfrastructureRepository(connectionString, connectionString.InfrastructureType, InfrastructureEntityGroups.ShrubMembers));

            var dataStorageModel = dataStorageBuilder.Build();

            Log.Information("Хранилище инициализировано.");

            return dataStorageModel;
        }

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion
    }
}
