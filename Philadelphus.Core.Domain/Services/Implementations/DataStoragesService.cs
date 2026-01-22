using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Json.Repositories;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Сервис работы с хранилищами данных
    /// </summary>
    public class DataStoragesService : IDataStoragesService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger<TreeRepositoryCollectionService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IOptions<ApplicationSettings> _applicationSettings;
        private readonly IOptions<ConnectionStringsCollection> _connectionStringsCollection;
        private readonly IOptions<DataStoragesCollection> _dataStoragesCollection;

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
            ILogger<TreeRepositoryCollectionService> logger,
            INotificationService notificationService,
            IOptions<ApplicationSettings> applicationSettings,
            IOptions<ConnectionStringsCollection> connectionStringsCollection,
            IOptions<DataStoragesCollection> dataStoragesCollection)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _applicationSettings = applicationSettings;
            _connectionStringsCollection = connectionStringsCollection;
            _dataStoragesCollection = dataStoragesCollection;

            _logger.LogInformation("DataStoragesService инициализирован.");
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить коллекцию хранилищ данных
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDataStorageModel> GetStoragesModels()
        {
            var connectionStrings = _connectionStringsCollection.Value.ConnectionStringContainers;
            var dbEntities = _dataStoragesCollection.Value.DataStorages;
            var models = new List<IDataStorageModel>();
            foreach (var dbEntity in dbEntities)
            {
                var cs = connectionStrings.SingleOrDefault(x => x.Uuid == dbEntity.Uuid);
                var model = dbEntity.ToModel(cs.ConnectionString);
                models.Add(model);
            }
            return models;
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
        public IDataStorageModel CreateMainDataStorageModel(
            FileInfo storagesConfigFullPath,
            FileInfo repositoryHeadersConfigFullPath)
        {
            if (storagesConfigFullPath == null || repositoryHeadersConfigFullPath == null)
                return null;

            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder()
                .SetGeneralParameters(
                    name: "Основное хранилище",
                    description: "Основное хранилище",
                    Guid.Empty,
                    InfrastructureTypes.JsonDocument,
                isDisabled: false)
            .SetRepository(new JsonDataStoragesCollectionInfrastructureRepository(storagesConfigFullPath))
            .SetRepository(new JsonTreeRepositoryHeadersCollectionInfrastructureRepository(repositoryHeadersConfigFullPath))
        ;
            var mainDataStorageModel = dataStorageBuilder.Build();

            _logger.LogInformation("Хранилище конфигурационных файлов инициализировано.");

            return mainDataStorageModel;
        }

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion
    }
}
