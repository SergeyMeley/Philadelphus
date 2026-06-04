using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts;

namespace Philadelphus.Tests.Domain.Infrastructure.Persistence.SQLite
{
    public class SqliteShrubMembersMigrationTests
    {
        [Fact]
        public async Task Migrate_CreatesTreeLeavesStringValueColumn()
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"philadelphus-shrub-members-{Guid.NewGuid():N}.db");
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Pooling = false
            }.ToString();

            try
            {
                await using (var context = new SqliteEfShrubMembersContext(connectionString))
                {
                    context.Database.GetMigrations().Should().Contain("20260522090000_AddTreeLeaveStringValue");
                    await context.Database.MigrateAsync();
                }

                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                var columns = new List<string>();
                await using (var columnsCommand = connection.CreateCommand())
                {
                    columnsCommand.CommandText = "PRAGMA table_info(tree_leaves);";

                    await using var reader = await columnsCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        columns.Add(reader.GetString(1));
                    }
                }

                columns.Should().Contain("string_value");

                await using var migrationCommand = connection.CreateCommand();
                migrationCommand.CommandText = """
                    SELECT COUNT(*)
                    FROM __EFMigrationsHistory
                    WHERE MigrationId = '20260522090000_AddTreeLeaveStringValue';
                    """;

                var appliedMigrationCount = await migrationCommand.ExecuteScalarAsync();
                appliedMigrationCount.Should().Be(1L);
            }
            finally
            {
                SqliteConnection.ClearAllPools();

                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
            }
        }
    }
}
