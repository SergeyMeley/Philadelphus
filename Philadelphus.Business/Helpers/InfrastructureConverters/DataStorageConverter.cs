using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.OtherEntities;
using Philadelphus.JsonRepository.Repositories;
using Philadelphus.PostgreEfRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureConverters.Converters
{
    public static class DataStorageConverter
    {
        public static DataStorage BusinessToDbEntity(this IDataStorageModel model)
        {
            var result = new DataStorage()
            {
                Name = model.Name,
                Description = model.Description,
                Guid = model.Guid,
                HasDataStorageInfrastructureRepositoryRepository = model.DataStorageInfrastructureRepositoryRepository != null,
                HasTreeRepositoryHeadersInfrastructureRepository = model.TreeRepositoryHeadersInfrastructureRepository != null,
                HasMainEntitiesInfrastructureRepository = model.MainEntitiesInfrastructureRepository != null,
                InfrastructureType = model.InfrastructureType,
            };
            return result;
        }
        public static IDataStorageModel DbToBusinessEntity(this DataStorage entity, string connectionString)
        {
            ITreeRepositoriesInfrastructureRepository treeRepositoryHeadersInfrastructureRepository = null;
            IMainEntitiesInfrastructureRepository mainEntitiesInfrastructureRepository = null;
            IDataStoragesInfrastructureRepository dataStorageInfrastructureRepository = null;
            switch (entity.InfrastructureType)
            {
                case InfrastructureTypes.WindowsDirectory:
                    break;
                case InfrastructureTypes.PostgreSqlAdo:
                    break;
                case InfrastructureTypes.PostgreSqlEf:
                    treeRepositoryHeadersInfrastructureRepository = new PostgreEfTreeRepositoryHeadersInfrastructureRepository(connectionString);
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
                    dataStorageInfrastructureRepository = new JsonDataStorageAndTreeRepositoryInfrastructureRepository();
                    treeRepositoryHeadersInfrastructureRepository = new JsonDataStorageAndTreeRepositoryInfrastructureRepository();
                    break;
                case InfrastructureTypes.XmlDocument:
                    break;
                default:
                    break;
            }
            var builder = new DataStorageBuilder()
                .SetGeneralParameters(entity.Name, entity.Description, entity.Guid, entity.InfrastructureType)
                .SetRepository(dataStorageInfrastructureRepository)
                .SetRepository(treeRepositoryHeadersInfrastructureRepository)
                .SetRepository(mainEntitiesInfrastructureRepository);
            return builder.Build();
        }
    }
}
