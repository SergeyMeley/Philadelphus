using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    public static class DataStorageConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static DataStorage ToDbEntity(
            this IDataStorageModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = new DataStorage()
            {
                Name = businessEntity.Name,
                Description = businessEntity.Description,
                Uuid = businessEntity.Uuid,
                HasPhiladelphusRepositoryHeadersInfrastructureRepository = businessEntity.PhiladelphusRepositoriesInfrastructureRepository != null,
                HasMainEntitiesInfrastructureRepository = businessEntity.PhiladelphusRepositoryMembersInfrastructureRepository != null,
                InfrastructureType = businessEntity.InfrastructureType,
            };
            return result;
        }

        /// <summary>
        /// Конвертировать сущность БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="connectionString">Строка подключения</param>
        /// <returns></returns>
        public static IDataStorageModel ToModel(
            this DataStorage dbEntity,
            ILogger logger,
            string connectionString)
        {
            if (dbEntity == null)
                return null;
            IPhiladelphusRepositoriesInfrastructureRepository PhiladelphusRepositoriesInfrastructureRepository = null;
            IPhiladelphusRepositoriesMembersInfrastructureRepository mainEntitiesInfrastructureRepository = null;
            switch (dbEntity.InfrastructureType)
            {
                case InfrastructureTypes.WindowsDirectory:
                    break;
                case InfrastructureTypes.PostgreSqlAdo:
                    break;
                case InfrastructureTypes.PostgreSqlEf:
                    PhiladelphusRepositoriesInfrastructureRepository = new PostgreEfPhiladelphusRepositoriesInfrastructureRepository(logger, connectionString);
                    mainEntitiesInfrastructureRepository = new PostgreEfMainEntitiesInfrastructureRepository(logger, connectionString);
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
                .SetGeneralParameters(logger, dbEntity.Name, dbEntity.Description, dbEntity.Uuid, dbEntity.InfrastructureType, dbEntity.IsDisabled)
                .SetRepository(PhiladelphusRepositoriesInfrastructureRepository)
                .SetRepository(mainEntitiesInfrastructureRepository);
            return builder.Build();
        }
    }
}
