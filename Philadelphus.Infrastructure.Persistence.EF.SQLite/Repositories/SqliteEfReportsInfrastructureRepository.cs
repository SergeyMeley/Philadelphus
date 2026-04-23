using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System.Data;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories
{
    public class SqliteEfReportsInfrastructureRepository : SqliteEfInfrastructureRepositoryBase<SqliteEfReportsContext>, IReportsInfrastructureRepository
    {
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.Reports; }

        public SqliteEfReportsInfrastructureRepository(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }

        protected override SqliteEfReportsContext GetNewContext() => new SqliteEfReportsContext(_connectionString);

        protected override DbSet<TEntity> GetDbSet<TEntity>(SqliteEfReportsContext context) where TEntity : class
        {
            return typeof(TEntity).Name switch
            {
                nameof(DynamicReportResult) => context.DynamicResults as DbSet<TEntity>,
                _ => throw new NotSupportedException($"Тип {typeof(TEntity).Name} не поддерживается.")
            };
        }

        protected override void SetNavigationProperties<TEntity, TNav>(TEntity item, Dictionary<Guid, TNav> navigationEntities)
        {
        }

        public async Task<List<ReportInfo>> GetAvailableReportsAsync(string schemaName)
        {
            var reports = new List<ReportInfo>();
            reports.AddRange(await GetViewsAsync());
            // SQLite не поддерживает materialized views и table-valued functions
            return reports;
        }

        private async Task<List<ReportInfo>> GetViewsAsync()
        {
            const string sql = @"
            SELECT name
            FROM sqlite_master
            WHERE type = 'view' AND name LIKE 'report_%'
            ORDER BY name";

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqliteCommand(sql, connection);

            var views = new List<ReportInfo>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                views.Add(new ReportInfo
                {
                    Name = reader.GetString(0),
                    Schema = "main",    // SQLite использует 'main' по умолчанию
                    Type = "View",
                    Description = null  // SQLite не хранит описания
                });
            }
            return views;
        }

        public async Task<DataTable> ExecuteReportAsync(ReportInfo report)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql;
            if (report.Type == "View")
            {
                sql = $"SELECT * FROM \"{report.Name}\"";
            }
            else
            {
                throw new ArgumentException("Некорректный тип отчета. SQLite поддерживает только views.");
            }

            using var cmd = new SqliteCommand(sql, connection);

            var result = new DataTable();
            using var reader = await cmd.ExecuteReaderAsync();
            result.Load(reader);
            return result;
        }

        public async Task RefreshMaterializedViewAsync(ReportInfo report)
        {
            // SQLite не поддерживает materialized views
            return;
        }
    }
}