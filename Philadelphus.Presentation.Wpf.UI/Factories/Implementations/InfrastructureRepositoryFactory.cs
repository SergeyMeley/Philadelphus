using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
{
    /// <summary>
    /// Фабрика создания репозитория.
    /// </summary>
    public class InfrastructureRepositoryFactory : IInfrastructureRepositoryFactory
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InfrastructureRepositoryFactory" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        public InfrastructureRepositoryFactory(
            ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="infrastructureType">Тип инфраструктуры.</param>
        /// <param name="entityGroup">Группа инфраструктурных сущностей.</param>
        /// <param name="connectionString">Строка подключения.</param>
        /// <returns>Созданный объект.</returns>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public IInfrastructureRepository Create(InfrastructureTypes infrastructureType, InfrastructureEntityGroups entityGroup, string connectionString)
        {
            switch (infrastructureType)
            {
                case InfrastructureTypes.PostgreSqlEf:
                    switch (entityGroup)
                    {
                        case InfrastructureEntityGroups.PhiladelphusRepositories:
                            return new PostgreEfPhiladelphusRepositoriesInfrastructureRepository(_logger, connectionString);
                        case InfrastructureEntityGroups.ShrubMembers:
                            return new PostgreEfShrubMembersInfrastructureRepository(_logger, connectionString);
                        case InfrastructureEntityGroups.Reports:
                            return new PostgreEfReportsInfrastructureRepository(_logger, connectionString);
                        default:
                            break;
                    }
                    break;
                case InfrastructureTypes.SQLiteEf:
                    switch (entityGroup)
                    {
                        case InfrastructureEntityGroups.PhiladelphusRepositories:
                            return new SqliteEfPhiladelphusRepositoriesInfrastructureRepository(_logger, connectionString);
                        case InfrastructureEntityGroups.ShrubMembers:
                            return new SqliteEfShrubMembersInfrastructureRepository(_logger, connectionString);
                        case InfrastructureEntityGroups.Reports:
                            return new SqliteEfReportsInfrastructureRepository(_logger, connectionString);
                        default:
                            break;
                    }
                    break;
                case InfrastructureTypes.JsonDocument:
                    break;
                default:
                    break;
            }
            throw new NotImplementedException();
        }
    }
}
