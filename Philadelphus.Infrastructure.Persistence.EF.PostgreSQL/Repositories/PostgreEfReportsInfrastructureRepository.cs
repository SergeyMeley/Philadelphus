using Microsoft.EntityFrameworkCore;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System.Data;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    /// <summary>
    /// Репозиторий доступа к данным отчета.
    /// </summary>
    public class PostgreEfReportsInfrastructureRepository : PostgreEfInfrastructureRepositoryBase<PostgreEfReportsContext>, IReportsInfrastructureRepository
    {
        /// <summary>
        /// Группа инфраструктурных сущностей.
        /// </summary>
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.Reports; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfReportsInfrastructureRepository" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="connectionString">Строка подключения.</param>
        public PostgreEfReportsInfrastructureRepository(
            ILogger logger,
            string connectionString) 
            : base(logger, connectionString)
        {
        }

        protected override PostgreEfReportsContext GetNewContext() => new PostgreEfReportsContext(_connectionString);

        protected override DbSet<TEntity> GetDbSet<TEntity>(PostgreEfReportsContext context) where TEntity : class
        {
            return typeof(TEntity).Name switch
            {
                nameof(DynamicReportResult) => context.DynamicResults as DbSet<TEntity>,
                _ => throw new NotSupportedException($"Тип {typeof(TEntity).Name} не поддерживается.")
            };
        }

        /// <summary>
        /// Получает данные отчета.
        /// </summary>
        /// <param name="schemaName">Имя схемы.</param>
        /// <returns>Задача, представляющая асинхронную операцию. Результат содержит возвращаемые данные.</returns>
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

                var pgType = reader.GetString(1);
                var dotNetType = PostgresTypeMapper.ToDotNetType(pgType);

                var param = new ReportParameter
                {
                    Name = reader.GetString(0),
                    Type = dotNetType,
                    DbType = pgType,
                    IsRequired = true,
                    Description = $"Тип {pgType}"
                };

                parameters.Add(param);
            }
            return parameters;
        }

        /// <summary>
        /// Выполняет операцию отчета.
        /// </summary>
        /// <param name="report">Отчет.</param>
        /// <returns>Задача, представляющая асинхронную операцию. Результат содержит возвращаемые данные.</returns>
        public async Task<DataTable> ExecuteReportAsync(ReportInfo report)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql;
            if (report.Type == "Function")
            {
                var paramNames = report.Parameters.Select(k => $"@{k.Name}") ?? Enumerable.Empty<string>();
                var paramList = string.Join(", ", paramNames);
                sql = $"SELECT * FROM \"{report.Schema}\".\"{report.Name}\"({paramList})";
            }
            else if (report.Type == "View" || report.Type == "MaterializedView")
            {
                sql = $"SELECT * FROM \"{report.Schema}\".\"{report.Name}\"";
            }
            else
            {
                throw new ArgumentException("Некорректный тип отчета.");
            }

            using var cmd = new NpgsqlCommand(sql, connection);
            if (report.Parameters != null)
            {
                foreach (var param in report.Parameters)
                {
                    var paramPair = new KeyValuePair<string, object>(param.Name, param.Value);
                    cmd.Parameters.Add(CreateParameter(paramPair, report));
                }
            }

            var result = new DataTable();
            using var reader = await cmd.ExecuteReaderAsync();
            result.Load(reader);
            return result;
        }

        /// <summary>
        /// Выполняет операцию RefreshMaterializedViewAsync.
        /// </summary>
        /// <param name="report">Отчет.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task RefreshMaterializedViewAsync(ReportInfo report)
        {
            if (report.Type != "MaterializedView")
                return;

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var concurrently = await HasValidUniqueIndexAsync(report.Schema, report.Name, connection) ? "CONCURRENTLY " : string.Empty;

            var sql = $"REFRESH MATERIALIZED VIEW { concurrently }\"{report.Schema}\".\"{report.Name}\"";
            using var cmd = new NpgsqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Асинхронно проверяет корректность уникального индекса.
        /// </summary>
        /// <param name="schema">Схема БД.</param>
        /// <param name="viewName">Имя представления БД.</param>
        /// <param name="connection">Подключение к БД.</param>
        /// <returns>Задача, представляющая асинхронную операцию. Результат содержит возвращаемые данные.</returns>
        public async Task<bool> HasValidUniqueIndexAsync(
            string schema,
            string viewName,
            NpgsqlConnection connection)
        {
            const string sql = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM pg_class t
                    JOIN pg_index i ON t.oid = i.indrelid
                    JOIN pg_namespace ns ON ns.oid = t.relnamespace
                    WHERE ns.nspname = @schema
                      AND t.relname = @viewName
                      AND i.indisunique = true
                      AND i.indpred IS NULL -- не partial index
                );";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("schema", schema);
            cmd.Parameters.AddWithValue("viewName", viewName);

            var result = (bool)await cmd.ExecuteScalarAsync();
            return result;
        }

        private NpgsqlParameter CreateParameter(KeyValuePair<string, object> param, ReportInfo report)
        {
            var paramMeta = report.Parameters?.FirstOrDefault(p => p.Name == param.Key);
            var paramType = paramMeta?.Type ?? param.Value?.GetType();

            object convertedValue = PostgresTypeMapper.ConvertValue(param.Value, paramType);

            var pgParam = new NpgsqlParameter($"@{param.Key}", convertedValue ?? DBNull.Value);

            if (paramType != null)
            {
                pgParam.NpgsqlDbType = PostgresTypeMapper.ToNpgsqlDbType(paramType);
            }

            return pgParam;
        }
    }
}
