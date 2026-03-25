using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.Repositories;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories
{
    public abstract class SqliteEfInfrastructureRepositoryBase<TContext> : EfInfrastructureRepositoryBase<TContext>
        where TContext : DbContext
    {
        protected SqliteEfInfrastructureRepositoryBase(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }
        protected override bool IsDuplicateTableException(Exception ex)
        {
            return ex is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 1;
        }

        //public override bool CheckAvailability()
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    using (var context = GetNewContext())
        //    {
        //        if (context.Database.CanConnect() == false)
        //            return false;

        //        try
        //        {
        //            var entityTypes = context.Model.GetEntityTypes().ToList();
        //            _logger.Information($"CheckAvailability: Найдено сущностей в модели {typeof(TContext).Name}: {entityTypes.Count}");
        //            if (!entityTypes.Any())
        //            {
        //                return false;
        //            }

        //            var expectedTables = entityTypes
        //                .Select(t => t.GetTableName())
        //                .Where(t => !string.IsNullOrEmpty(t))
        //                .Distinct()
        //                .ToList();

        //            if (!expectedTables.Any())
        //                return false;

        //            using (var connection = context.Database.GetDbConnection())
        //            {
        //                connection.Open();

        //                var existingTables = new List<string>();
        //                using (var command = connection.CreateCommand())
        //                {
        //                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
        //                    using (var reader = command.ExecuteReader())
        //                    {
        //                        while (reader.Read())
        //                        {
        //                            existingTables.Add(reader.GetString(0).ToLowerInvariant());
        //                        }
        //                    }
        //                }

        //                var missingTables = expectedTables
        //                    .Select(t => t.ToLowerInvariant())
        //                    .Except(existingTables)
        //                    .ToList();

        //                if (missingTables.Any())
        //                {
        //                    _logger.Debug($"Отсутствуют таблицы: {string.Join(", ", missingTables)}");
        //                    return false;
        //                }

        //                var testTable = expectedTables.First().ToLowerInvariant();
        //                using (var command = connection.CreateCommand())
        //                {
        //                    command.CommandText = $"SELECT COUNT(*) FROM \"{testTable}\" LIMIT 1";
        //                    command.ExecuteScalar();
        //                }

        //                connection.Close();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Debug($"CheckAvailability failed: {ex.Message}");
        //            return false;
        //        }

        //        sw.Stop();
        //        _logger.Information($"Task '{Task.CurrentId}'. Репозиторий БД '{this.GetType().Name}'. t = {sw.ElapsedMilliseconds} мс.");

        //        return true;
        //    }
        //}

        //protected override void InitDb()
        //{
        //    using (var context = GetNewContext())
        //    {
        //        if (CheckAvailabilityWithContext(context) == false)
        //        {
        //            _logger.Information($"Создание структуры БД для {typeof(TContext).Name}");

        //            // Принудительно создаем таблицы через SQL
        //            context.Database.EnsureCreated();

        //            // Проверяем, какие таблицы реально создались
        //            using (var connection = context.Database.GetDbConnection())
        //            {
        //                connection.Open();
        //                using (var command = connection.CreateCommand())
        //                {
        //                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
        //                    using (var reader = command.ExecuteReader())
        //                    {
        //                        var tables = new List<string>();
        //                        while (reader.Read())
        //                        {
        //                            tables.Add(reader.GetString(0));
        //                        }
        //                        _logger.Information($"Существующие таблицы после EnsureCreated: {string.Join(", ", tables)}");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //private bool CheckAvailabilityWithContext(TContext context)
        //{
        //    if (context.Database.CanConnect() == false)
        //        return false;

        //    try
        //    {
        //        var entityTypes = context.Model.GetEntityTypes().ToList();
        //        if (!entityTypes.Any())
        //        {
        //            return false;
        //        }

        //        var expectedTables = entityTypes
        //            .Select(t => t.GetTableName())
        //            .Where(t => !string.IsNullOrEmpty(t))
        //            .Distinct()
        //            .ToList();

        //        if (!expectedTables.Any())
        //            return false;

        //        using (var connection = context.Database.GetDbConnection())
        //        {
        //            connection.Open();

        //            var existingTables = new List<string>();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
        //                using (var reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        existingTables.Add(reader.GetString(0).ToLowerInvariant());
        //                    }
        //                }
        //            }

        //            var missingTables = expectedTables
        //                .Select(t => t.ToLowerInvariant())
        //                .Except(existingTables)
        //                .ToList();

        //            if (missingTables.Any())
        //            {
        //                return false;
        //            }

        //            connection.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Debug($"CheckAvailabilityWithContext failed: {ex.Message}");
        //        return false;
        //    }

        //    return true;
        //}

        protected virtual bool IsMigrationNotSupportedException(NotSupportedException ex)
        {
            return ex.Message.Contains("SQLite") ||
                   ex.Message.Contains("schema") ||
                   ex.Message.Contains("rebuild");
        }
    }
}