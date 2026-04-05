using Microsoft.EntityFrameworkCore;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System.Data;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    public class PostgreEfReportsInfrastructureRepository : PostgreEfInfrastructureRepositoryBase<PostgreEfReportsContext>, IReportsInfrastructureRepository
    {
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.ShrubMembers; }

        public PostgreEfReportsInfrastructureRepository(
            ILogger logger,
            string connectionString) 
            : base(logger, connectionString)
        {
        }

        protected override PostgreEfReportsContext GetNewContext() => new PostgreEfReportsContext(_connectionString);

        public async Task<List<ReportInfo>> GetAvailableReportsAsync(string schemaName)
        {
            var reports = new List<ReportInfo>();
            reports.AddRange(await GetViewsAsync(schemaName));
            reports.AddRange(await GetMaterializedViewsAsync(schemaName));
            reports.AddRange(await GetTableFunctionsAsync(schemaName));
            return reports;
        }

        private async Task<List<ReportInfo>> GetViewsAsync(string schema)
        {
            const string sql = @"
            SELECT table_name, 
                   obj_description((table_schema || '.' || table_name)::regclass) as description
            FROM information_schema.views
            WHERE table_schema = @schema
            ORDER BY table_name";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@schema", schema);

            var views = new List<ReportInfo>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                views.Add(new ReportInfo
                {
                    Name = reader.GetString(0),
                    Schema = schema,
                    Type = "View",
                    Description = reader.IsDBNull(1) ? null : reader.GetString(1)
                });
            }
            return views;
        }

        private async Task<List<ReportInfo>> GetMaterializedViewsAsync(string schema)
        {
            const string sql = @"
            SELECT matviewname,
                   obj_description((schemaname || '.' || matviewname)::regclass) as description
            FROM pg_matviews
            WHERE schemaname = @schema
            ORDER BY matviewname";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@schema", schema);

            var mv = new List<ReportInfo>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                mv.Add(new ReportInfo
                {
                    Name = reader.GetString(0),
                    Schema = schema,
                    Type = "MaterializedView",
                    Description = reader.IsDBNull(1) ? null : reader.GetString(1)
                });
            }
            return mv;
        }

        private async Task<List<ReportInfo>> GetTableFunctionsAsync(string schema)
        {
            const string sql = @"
            SELECT p.proname, obj_description(p.oid) as description
            FROM pg_proc p
            JOIN pg_namespace n ON p.pronamespace = n.oid
            WHERE n.nspname = @schema
              AND p.prokind = 'f'
              AND p.proretset = true
            ORDER BY p.proname";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@schema", schema);

            var functions = new List<ReportInfo>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var func = new ReportInfo
                {
                    Name = reader.GetString(0),
                    Schema = schema,
                    Type = "Function",
                    Description = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Parameters = await GetFunctionParametersAsync(schema, reader.GetString(0))
                };
                functions.Add(func);
            }
            return functions;
        }

        private async Task<List<ReportParameter>> GetFunctionParametersAsync(string schema, string functionName)
        {
            const string sql = @"
            SELECT unnest(p.proargnames) as name,
                   unnest(p.proargtypes)::regtype::text as type_name,
                   p.proargmodes
            FROM pg_proc p
            JOIN pg_namespace n ON p.pronamespace = n.oid
            WHERE n.nspname = @schema AND p.proname = @func";

            var parameters = new List<ReportParameter>();
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@schema", schema);
            cmd.Parameters.AddWithValue("@func", functionName);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var mode = reader.IsDBNull(2) ? "i" : reader.GetString(2);
                if (mode != "i") continue;

                var param = new ReportParameter
                {
                    Name = reader.GetString(0),
                    Type = MapPostgresTypeToDotNetType(reader.GetString(1)),
                    IsRequired = true,
                    Description = $"Параметр {reader.GetString(0)}"
                };
                parameters.Add(param);
            }
            return parameters;
        }

        private Type MapPostgresTypeToDotNetType(string pgType)
        {
            return pgType.ToLower() switch
            {
                "integer" or "int4" => typeof(int),
                "bigint" or "int8" => typeof(long),
                "text" or "varchar" or "character varying" => typeof(string),
                "boolean" or "bool" => typeof(bool),
                "date" => typeof(DateTime),
                "timestamp" or "timestamptz" => typeof(DateTime),
                "numeric" or "decimal" => typeof(decimal),
                "double precision" => typeof(double),
                "real" => typeof(float),
                "uuid" => typeof(Guid),
                _ => typeof(string)
            };
        }

        public async Task<DataTable> ExecuteReportAsync(ReportInfo report, Dictionary<string, object> parameters)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql;
            if (report.Type == "Function")
            {
                var paramNames = parameters?.Keys.Select(k => $"@{k}") ?? Enumerable.Empty<string>();
                var paramList = string.Join(", ", paramNames);
                sql = $"SELECT * FROM \"{report.Schema}\".\"{report.Name}\"({paramList})";
            }
            else // View или MaterializedView
            {
                sql = $"SELECT * FROM \"{report.Schema}\".\"{report.Name}\"";
            }

            using var cmd = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue($"@{p.Key}", p.Value ?? DBNull.Value);
                }
            }

            var result = new DataTable();
            using var reader = await cmd.ExecuteReaderAsync();
            result.Load(reader);
            return result;
        }

        public async Task RefreshMaterializedViewAsync(ReportInfo report)
        {
            if (report.Type != "MaterializedView")
                throw new InvalidOperationException("Can refresh only materialized view");

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            var sql = $"REFRESH MATERIALIZED VIEW CONCURRENTLY \"{report.Schema}\".\"{report.Name}\"";
            using var cmd = new NpgsqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        protected override DbSet<TEntity> GetDbSet<TEntity>(PostgreEfReportsContext context) where TEntity : class
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
    }
}
