using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System.Data;
using System.IO;

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
            reports.AddRange(await GetTablesAsync());
            return reports;
        }

        public async Task<DataTable> ExecuteReportAsync(ReportInfo report)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Убедиться, что таблица метаданных существует
            await EnsureMetadataTableExistsAsync(connection);

            // Для таблиц-отчетов получить метаданные об ATTACH из system_report_metadata
            if (report.Type == "MaterializedView" && string.IsNullOrEmpty(report.AttachDatabases))
            {
                const string getMetadataSql = @"
                SELECT attach_databases, creation_sql FROM system_report_metadata WHERE report_name = @reportName";

                using var metadataCmd = new SqliteCommand(getMetadataSql, connection);
                metadataCmd.Parameters.AddWithValue("@reportName", report.Name);
                var attachDatabases = await metadataCmd.ExecuteScalarAsync() as string;
                if (!string.IsNullOrEmpty(attachDatabases))
                {
                    report.AttachDatabases = attachDatabases;
                }
            }

            // Прикрепить требуемые базы данных, если они указаны
            if (!string.IsNullOrEmpty(report.AttachDatabases))
            {
                await AttachDatabases(connection, report.AttachDatabases);
            }

            string sql;
            if (report.Type == "View" || report.Type == "MaterializedView")
            {
                sql = $"SELECT * FROM \"{report.Name}\"";
            }
            else
            {
                throw new ArgumentException($"Некорректный тип отчета '{report.Type}'. SQLite поддерживает только View и MaterializedView (CREATE TABLE AS SELECT).");
            }

            using var cmd = new SqliteCommand(sql, connection);

            var result = new DataTable();
            using var reader = await cmd.ExecuteReaderAsync();
            result.Load(reader);
            return result;
        }

        public async Task RefreshMaterializedViewAsync(ReportInfo report)
        {
            // Для SQLite "материализованные представления" - это таблицы, созданные через CREATE TABLE AS SELECT
            if (report.Type != "MaterializedView")
                return;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Убедиться, что таблица метаданных существует
            await EnsureMetadataTableExistsAsync(connection);

            try
            {
                // Загрузить полные метаданные из system_report_metadata
                string attachDatabases = null;
                string creationSql = null;

                const string getMetadataSql = @"
                SELECT attach_databases, creation_sql FROM system_report_metadata WHERE report_name = @reportName";

                using (var metadataCmd = new SqliteCommand(getMetadataSql, connection))
                {
                    metadataCmd.Parameters.AddWithValue("@reportName", report.Name);
                    using var reader = await metadataCmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        attachDatabases = reader.IsDBNull(0) ? null : reader.GetString(0);
                        creationSql = reader.IsDBNull(1) ? null : reader.GetString(1);
                    }
                }

                // Использовать загруженные метаданные если они есть
                if (!string.IsNullOrEmpty(attachDatabases))
                {
                    report.AttachDatabases = attachDatabases;
                }

                // Прикрепить требуемые базы данных, если они указаны
                if (!string.IsNullOrEmpty(report.AttachDatabases))
                {
                    _logger?.Information($"Прикрепление БД для отчета '{report.Name}': {report.AttachDatabases}");
                    await AttachDatabases(connection, report.AttachDatabases);
                }

                // Использовать сохранённый SQL если он есть, иначе получить из sqlite_master
                if (string.IsNullOrEmpty(creationSql))
                {
                    _logger?.Information($"creation_sql не найден в метаданных для '{report.Name}'. Восстанавливаю из sqlite_master...");

                    const string getSqlCommand = @"
                    SELECT sql
                    FROM sqlite_master
                    WHERE type = 'table' AND name = @name";

                    using var cmd = new SqliteCommand(getSqlCommand, connection);
                    cmd.Parameters.AddWithValue("@name", report.Name);
                    creationSql = (string?)await cmd.ExecuteScalarAsync();

                    // Если это только CREATE TABLE без SELECT - это ошибка (потеряна SELECT часть)
                    if (!string.IsNullOrEmpty(creationSql) && !creationSql.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger?.Error($"КРИТИЧЕСКАЯ ОШИБКА: SQL для таблицы '{report.Name}' не содержит SELECT! " +
                            $"Таблица была создана без сохранения полного SQL в метаданных. " +
                            $"Необходимо вручную добавить creation_sql в system_report_metadata: " +
                            $"UPDATE system_report_metadata SET creation_sql = 'ПОЛНЫЙ SQL' WHERE report_name = '{report.Name}'");
                        throw new InvalidOperationException(
                            $"Не удалось восстановить полный SQL для таблицы '{report.Name}'. " +
                            $"Исходный SQL с SELECT был потерян. Обновить метаданные вручную.");
                    }
                }

                if (string.IsNullOrEmpty(creationSql))
                {
                    _logger?.Warning($"Не найден SQL для таблицы '{report.Name}'");
                    return;
                }

                // Очистить SQL от комментариев перед выполнением
                var cleanSql = RemoveComments(creationSql);

                _logger?.Information($"Пересоздание таблицы '{report.Name}' с SQL: {cleanSql}");

                // Пересоздать таблицу: сначала удалить, потом создать заново
                using (var deleteCmd = new SqliteCommand($"DROP TABLE IF EXISTS \"{report.Name}\"", connection))
                {
                    await deleteCmd.ExecuteNonQueryAsync();
                }

                using (var recreateCmd = new SqliteCommand(cleanSql, connection))
                {
                    await recreateCmd.ExecuteNonQueryAsync();
                }

                // Сохранить полные метаданные (ATTACH и creation_sql) в system_report_metadata
                // Нормализовать creation_sql: удалить комментарии и переносы строк для компактного хранения
                var normalizedCreationSql = NormalizeSqlForStorage(creationSql);

                const string upsertMetadata = @"
                    INSERT OR REPLACE INTO system_report_metadata (report_name, attach_databases, creation_sql)
                    VALUES (@reportName, @attachDatabases, @creationSql);";

                using var upsertCmd = new SqliteCommand(upsertMetadata, connection);
                upsertCmd.Parameters.AddWithValue("@reportName", report.Name);
                upsertCmd.Parameters.AddWithValue("@attachDatabases", (object)attachDatabases ?? DBNull.Value);
                // Сохранять нормализованный SQL без комментариев и переносов строк
                upsertCmd.Parameters.AddWithValue("@creationSql", (object)normalizedCreationSql ?? DBNull.Value);
                await upsertCmd.ExecuteNonQueryAsync();

                _logger?.Information($"Таблица '{report.Name}' успешно пересоздана. Метаданные сохранены.");
                _logger?.Information($"Сохранён creation_sql: {normalizedCreationSql.Substring(0, Math.Min(150, normalizedCreationSql.Length))}...");
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Ошибка при обновлении материализованного представления '{report.Name}'. " +
                    $"Возможно, требуется ATTACH DATABASE для доступа к другой БД.",
                    ex);
            }
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

        private async Task<List<ReportInfo>> GetTablesAsync()
        {
            const string sql = @"
            SELECT t.name, t.sql, m.attach_databases
            FROM sqlite_master t
            LEFT JOIN system_report_metadata m ON t.name = m.report_name
            WHERE t.type = 'table' AND t.name LIKE 'report_%'
            ORDER BY t.name";

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            // Убедиться, что таблица метаданных существует
            await EnsureMetadataTableExistsAsync(connection);
            
            using var cmd = new SqliteCommand(sql, connection);

            var tables = new List<ReportInfo>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var tableName = reader.GetString(0);
                var tableSql = reader.IsDBNull(1) ? null : reader.GetString(1);
                var attachDatabases = reader.IsDBNull(2) ? null : reader.GetString(2);

                tables.Add(new ReportInfo
                {
                    Name = tableName,
                    Schema = "main",
                    Type = "MaterializedView",  // В SQLite - это таблица, но логически это материализованное представление
                    Description = null,
                    AttachDatabases = attachDatabases
                });
            }
            return tables;
        }

        private async Task EnsureMetadataTableExistsAsync(SqliteConnection connection)
        {
            const string createMetadataTable = @"
            CREATE TABLE IF NOT EXISTS system_report_metadata (
                report_name TEXT PRIMARY KEY,
                attach_databases TEXT,
                creation_sql TEXT
            );";

            using var cmd = new SqliteCommand(createMetadataTable, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task AttachDatabases(SqliteConnection connection, string attachDatabasesInfo)
        {
            if (string.IsNullOrEmpty(attachDatabasesInfo))
                return;

            // Получить список уже прикрепленных БД
            var attachedDatabases = new HashSet<string>();
            using (var pragmaCmd = new SqliteCommand("PRAGMA database_list;", connection))
            {
                using var reader = await pragmaCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dbName = reader.GetString(1); // seq, name, file
                    attachedDatabases.Add(dbName);
                }
            }

            // Парсирование формата "alias1=path1;alias2=path2"
            var attachments = attachDatabasesInfo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var attachment in attachments)
            {
                var parts = attachment.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                var alias = parts[0].Trim();
                var path = parts[1].Trim();

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(
                        $"БД '{alias}' не найдена по пути: {path}");
                }

                // Прикрепить БД только если она еще не прикреплена
                if (!attachedDatabases.Contains(alias))
                {
                    _logger?.Information($"Прикрепление БД '{alias}' по пути '{path}'");
                    using var attachCmd = new SqliteCommand(
                        $"ATTACH DATABASE '{path}' AS {alias};", connection);
                    await attachCmd.ExecuteNonQueryAsync();
                }
                else
                {
                    _logger?.Information($"БД '{alias}' уже прикреплена");
                }
            }
        }

        private string RemoveComments(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return sql;

            // Удалить многострочные комментарии /* ... */
            var result = System.Text.RegularExpressions.Regex.Replace(sql, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
            
            // Удалить однострочные комментарии -- ...
            result = System.Text.RegularExpressions.Regex.Replace(result, @"--.*?$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            
            // Удалить лишние пробелы и пустые строки
            result = System.Text.RegularExpressions.Regex.Replace(result, @"^\s*$[\r\n]*", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            
            return result.Trim();
        }

        private string NormalizeSqlForStorage(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return sql;

            // Сначала удалить комментарии
            var result = RemoveComments(sql);
            
            // Заменить все переносы строк и табуляции на пробелы
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[\r\n\t]+", " ");
            
            // Нормализовать множественные пробелы на единичные
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
            
            return result.Trim();
        }
    }
}