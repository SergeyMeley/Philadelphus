using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Core.Domain.Identity.Services.Interfaces;

using Serilog;

namespace Philadelphus.Presentation.Avalonia.Factories
{
    /// <summary>
    /// Фабрика создания инфраструктурного репозитория.
    /// Зависит от конкретных Persistence.EF.PostgreSQL/SQLite, поэтому живёт в точке композиции
    /// (как и WPF-версия), а не в shared-слое.
    /// </summary>
    public class InfrastructureRepositoryFactory : IInfrastructureRepositoryFactory
    {
        private readonly ILogger _logger;
        private readonly IUserService _userService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InfrastructureRepositoryFactory" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        public InfrastructureRepositoryFactory(
            ILogger logger,
            IUserService userService)
        {
            ArgumentNullException.ThrowIfNull(userService);

            _logger = logger;
            _userService = userService;
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
            var auditUserName = _userService.CurrentUser.UserName;

            switch (infrastructureType)
            {
                case InfrastructureTypes.PostgreSqlEf:
                    switch (entityGroup)
                    {
                        case InfrastructureEntityGroups.PhiladelphusRepositories:
                            return new PostgreEfPhiladelphusRepositoriesInfrastructureRepository(_logger, connectionString, auditUserName);
                        case InfrastructureEntityGroups.ShrubMembers:
                            return new PostgreEfShrubMembersInfrastructureRepository(_logger, connectionString, auditUserName);
                        case InfrastructureEntityGroups.Reports:
                            return new PostgreEfReportsInfrastructureRepository(_logger, connectionString, auditUserName);
                        default:
                            break;
                    }
                    break;
                case InfrastructureTypes.SQLiteEf:
                    switch (entityGroup)
                    {
                        case InfrastructureEntityGroups.PhiladelphusRepositories:
                            return new SqliteEfPhiladelphusRepositoriesInfrastructureRepository(_logger, connectionString, auditUserName);
                        case InfrastructureEntityGroups.ShrubMembers:
                            return new SqliteEfShrubMembersInfrastructureRepository(_logger, connectionString, auditUserName);
                        case InfrastructureEntityGroups.Reports:
                            return new SqliteEfReportsInfrastructureRepository(_logger, connectionString, auditUserName);
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
