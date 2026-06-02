using FluentAssertions;
using Microsoft.Data.Sqlite;
using Philadelphus.Infrastructure.Persistence.EF.SQLite;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;

namespace Philadelphus.Tests.Domain.Infrastructure.Persistence.SQLite
{
    public class SqliteReportSqlBuilderTests
    {
        [Fact]
        public void BuildExecuteReportSql_Escapes_Report_Name()
        {
            var report = new ReportInfo
            {
                Name = "report_sales\"; DROP TABLE users; --",
                Type = "View"
            };

            var result = SqliteReportSqlBuilder.BuildExecuteReportSql(report);

            result.Should().Be("SELECT * FROM \"report_sales\"\"; DROP TABLE users; --\"");
        }

        [Fact]
        public void BuildDropTableSql_Escapes_Report_Name()
        {
            var report = new ReportInfo
            {
                Name = "report_sales\"; DROP TABLE users; --",
                Type = "MaterializedView"
            };

            var result = SqliteReportSqlBuilder.BuildDropTableSql(report);

            result.Should().Be("DROP TABLE IF EXISTS \"report_sales\"\"; DROP TABLE users; --\"");
        }

        [Fact]
        public void BuildAttachDatabaseSql_Escapes_Alias_And_Uses_Path_Parameter()
        {
            var result = SqliteReportSqlBuilder.BuildAttachDatabaseSql("other\"; DETACH DATABASE main; --");

            result.Should().Be("ATTACH DATABASE @path AS \"other\"\"; DETACH DATABASE main; --\";");
        }

        [Fact]
        public async Task BuildAttachDatabaseSql_Attaches_Database_With_Parameterized_Path()
        {
            var mainPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
            var attachedPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}'; VACUUM; --.db");
            var alias = "other\"; DETACH DATABASE main; --";

            try
            {
                var attachedConnectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = attachedPath,
                    Pooling = false
                }.ToString();
                var mainConnectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = mainPath,
                    Pooling = false
                }.ToString();

                await using (var attachedConnection = new SqliteConnection(attachedConnectionString))
                {
                    await attachedConnection.OpenAsync();
                }

                await using var connection = new SqliteConnection(mainConnectionString);
                await connection.OpenAsync();

                await using var attachCommand = new SqliteCommand(
                    SqliteReportSqlBuilder.BuildAttachDatabaseSql(alias),
                    connection);
                attachCommand.Parameters.AddWithValue(SqliteReportSqlBuilder.AttachDatabasePathParameterName, attachedPath);

                await attachCommand.ExecuteNonQueryAsync();

                await using var listCommand = new SqliteCommand("PRAGMA database_list;", connection);
                await using var reader = await listCommand.ExecuteReaderAsync();

                var attachedAliases = new List<string>();
                while (await reader.ReadAsync())
                {
                    attachedAliases.Add(reader.GetString(1));
                }

                attachedAliases.Should().Contain(alias);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
                File.Delete(mainPath);
                File.Delete(attachedPath);
            }
        }

        [Fact]
        public void BuildExecuteReportSql_Rejects_Unsupported_Report_Type()
        {
            var report = new ReportInfo
            {
                Name = "report_sales",
                Type = "Function"
            };

            var act = () => SqliteReportSqlBuilder.BuildExecuteReportSql(report);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BuildExecuteReportSql_Rejects_Null_Report_Name()
        {
            var report = new ReportInfo
            {
                Name = null!,
                Type = "View"
            };

            var act = () => SqliteReportSqlBuilder.BuildExecuteReportSql(report);

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("bad\0name")]
        public void BuildExecuteReportSql_Rejects_Invalid_Report_Name(string reportName)
        {
            var report = new ReportInfo
            {
                Name = reportName,
                Type = "View"
            };

            var act = () => SqliteReportSqlBuilder.BuildExecuteReportSql(report);

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("bad\0alias")]
        public void BuildAttachDatabaseSql_Rejects_Invalid_Alias(string alias)
        {
            var act = () => SqliteReportSqlBuilder.BuildAttachDatabaseSql(alias);

            act.Should().Throw<ArgumentException>();
        }
    }
}
