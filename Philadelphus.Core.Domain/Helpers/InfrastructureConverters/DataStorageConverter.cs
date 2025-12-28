using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using Philadelphus.Infrastructure.Persistence.Json.Repositories;
using Philadelphus.PostgreEfRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureConverters.Converters
{
    public static class DataStorageConverter
    {
        public static DataStorage ToDbEntity(this IDataStorageModel model)
        {
            var result = new DataStorage()
            {
                Name = model.Name,
                Description = model.Description,
                Guid = model.Guid,
                HasDataStorageInfrastructureRepositoryRepository = model.DataStoragesCollectionInfrastructureRepository != null,
                HasTreeRepositoryHeadersInfrastructureRepository = model.TreeRepositoriesInfrastructureRepository != null,
                HasMainEntitiesInfrastructureRepository = model.MainEntitiesInfrastructureRepository != null,
                InfrastructureType = model.InfrastructureType,
            };
            return result;
        }
        public static IDataStorageModel ToModel(this DataStorage entity, string connectionString, DirectoryInfo directory)
        {
            IDataStoragesCollectionInfrastructureRepository dataStoragesCollectionInfrastructureRepository = new JsonDataStoragesCollectionInfrastructureRepository(directory);
            ITreeRepositoryHeadersCollectionInfrastructureRepository treeRepositoryHeadersCollectionInfrastructureRepository = new JsonTreeRepositoryHeadersCollectionInfrastructureRepository(directory);
            ITreeRepositoriesInfrastructureRepository treeRepositoriesInfrastructureRepository = null;
            IMainEntitiesInfrastructureRepository mainEntitiesInfrastructureRepository = null;
            switch (entity.InfrastructureType)
            {
                case InfrastructureTypes.WindowsDirectory:
                    break;
                case InfrastructureTypes.PostgreSqlAdo:
                    break;
                case InfrastructureTypes.PostgreSqlEf:
                    treeRepositoriesInfrastructureRepository = new PostgreEfTreeRepositoriesInfrastructureRepository(connectionString);
                    mainEntitiesInfrastructureRepository = new PostgreEfMainEntitiesInfrastructureRepository(connectionString);
                    break;
                case InfrastructureTypes.MongoDbAdo:
                    break;
                case InfrastructureTypes.MongoDbEf:
                    break;
                case InfrastructureTypes.MsSqlServerEf:
                    break;
                case InfrastructureTypes.SQLite:
                    break;
                case InfrastructureTypes.JsonDocument:
                    break;
                case InfrastructureTypes.XmlDocument:
                    break;
                default:
                    break;
            }
            var builder = new DataStorageBuilder()
                .SetGeneralParameters(entity.Name, entity.Description, entity.Guid, entity.InfrastructureType, entity.IsDisabled)
                .SetRepository(dataStoragesCollectionInfrastructureRepository)
                .SetRepository(treeRepositoryHeadersCollectionInfrastructureRepository)
                .SetRepository(treeRepositoriesInfrastructureRepository)
                .SetRepository(mainEntitiesInfrastructureRepository);
            return builder.Build();
        }
    }
}
