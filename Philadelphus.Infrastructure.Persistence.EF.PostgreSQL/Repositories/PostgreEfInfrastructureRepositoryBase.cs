using Microsoft.EntityFrameworkCore;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.EF.Repositories;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    /// <summary>
    /// Представляет объект базового репозитория БД PostgreSQL.
    /// </summary>
    public abstract class PostgreEfInfrastructureRepositoryBase<TContext> : EfInfrastructureRepositoryBase<TContext>
        where TContext : DbContext
    {
        protected PostgreEfInfrastructureRepositoryBase(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }
        protected override bool IsDuplicateTableException(Exception ex)
        {
            return ex is PostgresException pgEx && pgEx.SqlState == "42P07";
        }
    }
}