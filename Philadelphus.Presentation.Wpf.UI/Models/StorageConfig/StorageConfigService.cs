using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.PostgreEfRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Models.StorageConfig
{
    public class StorageConfigService : IStorageConfigService
    {
        private StorageConfig? _config;

        public StorageConfig LoadConfig(string filePath = "storage-config.json")
        {
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Philadelphus\\storage-config.json");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            _config = JsonSerializer.Deserialize<StorageConfig>(json, options);
            return _config ?? throw new InvalidOperationException("Failed to deserialize configuration");
        }

        public IEnumerable<IDataStorageModel> GetAllStorageModels()
        {
            var result = new List<IDataStorageModel>();
            foreach (var item in _config.DataStorageModels)
            {
                result.Add(CreateStorageModel(item.Guid));
            }
            return result;
        }
        public IDataStorageModel? CreateStorageModel(Guid modelGuid)
        {
            var modelConfig = _config?.DataStorageModels.FirstOrDefault(m => m.Guid == modelGuid);
            if (modelConfig == null) 
                return null;
            ITreeRepositoriesInfrastructureRepository treeRepositoryHeadersInfrastructureRepository = null;
            IMainEntitiesInfrastructureRepository mainEntitiesInfrastructureRepository = null;
            switch (modelConfig.ProviderType)
            {
                case InfrastructureTypes.WindowsDirectory:
                    break;
                case InfrastructureTypes.PostgreSqlAdo:
                    break;
                case InfrastructureTypes.PostgreSqlEf:
                    treeRepositoryHeadersInfrastructureRepository = new PostgreEfTreeRepositoriesInfrastructureRepository(ConfigurationManager.ConnectionStrings[modelConfig.Guid.ToString()].ConnectionString);
                    mainEntitiesInfrastructureRepository = new PostgreEfMainEntitiesInfrastructureRepository(ConfigurationManager.ConnectionStrings[modelConfig.Guid.ToString()].ConnectionString);
                    break;
                case InfrastructureTypes.MongoDbAdo:
                    break;
                default:
                    break;
            }
            if (treeRepositoryHeadersInfrastructureRepository == null || mainEntitiesInfrastructureRepository == null)
            {
                return null;
            }
            DataStorageBuilder builder = new DataStorageBuilder();
            builder
                .SetGeneralParameters(modelConfig.Name, modelConfig.Description, modelConfig.Guid, modelConfig.ProviderType, modelConfig.IsHidden)
                .SetRepository(treeRepositoryHeadersInfrastructureRepository)
                .SetRepository(mainEntitiesInfrastructureRepository);
            return builder.Build();
        }

        //public IDataStorageModel? GetDefaultStorageModel()
        //{
        //    if (_config == null) return null;
        //    return CreateStorageModel(_config.DefaultStorageGuid);
        //}

        //public List<IDataStorageModel> GetEnabledStorageModels()
        //{
        //    if (_config == null) return new List<IDataStorageModel>();

        //    return _config.DataStorageModels
        //        .Where(m => m.IsEnabled)
        //        .OrderBy(m => m.Priority)
        //        .Select(m => CreateStorageModel(m.Guid)!)
        //        .Where(m => m != null)
        //        .ToList();
        //}
    }
}
