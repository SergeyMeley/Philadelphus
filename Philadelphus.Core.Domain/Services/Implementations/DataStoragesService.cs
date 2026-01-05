using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Json.Repositories;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    public class DataStoragesService : IDataStoragesService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger<TreeRepositoryCollectionService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IDataStorageModel _mainDataStorageModel;

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Серсис работы с коллекцией репозиториев и хранилищами данных
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Сервис логгирования</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        /// <param name="treeRepositoryService">Сервис для работы с репозиторием и его элементами</param>
        public DataStoragesService(
            IMapper mapper,
            ILogger<TreeRepositoryCollectionService> logger,
            INotificationService notificationService,
            ITreeRepositoryService treeRepositoryService,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;

            _logger.LogInformation("DataStoragesService инициализирован.");

            _mainDataStorageModel = CreateMainDataStorageModel(applicationSettings.Value.StoragesConfigFullPath, applicationSettings.Value.RepositoryHeadersConfigFullPath);
            _logger.LogInformation("Основное хранилище конфигурационных файлов инициализировано.");
        }

        #endregion

        #region [ Get + Load ]

        public IEnumerable<IDataStorageModel> GetStoragesModels(ConnectionStringsCollection connectionStrings)
        {
            var dbEntities = _mainDataStorageModel.DataStoragesCollectionInfrastructureRepository.SelectDataStorages();
            var models = new List<IDataStorageModel>();
            foreach (var dbEntity in dbEntities)
            {
                var cs = connectionStrings.ConnectionStringContainers.SingleOrDefault(x => x.Uuid == dbEntity.Uuid);
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
        /// Сохдать хранилище данных
        /// </summary>
        /// <param name="configsDirectory">Путь к настроечному файлу</param>
        /// <returns>Хранилище данных</returns>
        public IDataStorageModel CreateMainDataStorageModel(
            FileInfo storagesConfigFullPath,
            FileInfo repositoryHeadersConfigFullPath)
        {
            if (storagesConfigFullPath == null || repositoryHeadersConfigFullPath == null)
                return null;

            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder()
                .SetGeneralParameters(
                    name: "Основное хранилище настроечных файлов",
                    description: "Хранилище настроечных файлов и конфигураций в формате json-документов",
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
