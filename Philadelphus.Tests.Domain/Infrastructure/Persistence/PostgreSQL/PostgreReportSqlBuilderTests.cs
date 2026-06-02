using FluentAssertions;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;

namespace Philadelphus.Tests.Domain.Infrastructure.Persistence.PostgreSQL
{
    public class PostgreReportSqlBuilderTests
    {
        [Fact]
        public void BuildExecuteReportSql_Escapes_View_Identifiers()
        {
            var report = new ReportInfo
            {
                Schema = "public\"; DROP SCHEMA secret; --",
                Name = "sales\"; DROP TABLE users; --",
                Type = "View"
            };

            var result = PostgreReportSqlBuilder.BuildExecuteReportSql(report);

            result.CommandText.Should().Be(
                "SELECT * FROM \"public\"\"; DROP SCHEMA secret; --\".\"sales\"\"; DROP TABLE users; --\"");
            result.ParameterNames.Should().BeEmpty();
        }

        [Fact]
        public void BuildExecuteReportSql_Uses_Generated_Function_Parameters()
        {
            var report = new ReportInfo
            {
                Schema = "reporting",
                Name = "get_sales",
                Type = "Function",
                Parameters =
                [
                    new ReportParameter { Name = "safe_name", Value = 1 },
                    new ReportParameter { Name = "bad); DROP TABLE users; --", Value = 2 }
                ]
            };

            var result = PostgreReportSqlBuilder.BuildExecuteReportSql(report);

            result.CommandText.Should().Be("SELECT * FROM \"reporting\".\"get_sales\"(@p0, @p1)");
            result.CommandText.Should().NotContain("safe_name");
            result.CommandText.Should().NotContain("DROP TABLE");
            result.ParameterNames.Should().Equal("@p0", "@p1");
        }

        [Fact]
        public void BuildRefreshMaterializedViewSql_Escapes_Identifiers_With_Concurrently()
        {
            var report = new ReportInfo
            {
                Schema = "public\"; DROP SCHEMA secret; --",
                Name = "mv\"; DROP TABLE users; --",
                Type = "MaterializedView"
            };

            var result = PostgreReportSqlBuilder.BuildRefreshMaterializedViewSql(report, concurrently: true);

            result.Should().Be(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY \"public\"\"; DROP SCHEMA secret; --\".\"mv\"\"; DROP TABLE users; --\"");
        }

        [Fact]
        public void BuildExecuteReportSql_Rejects_Unsupported_Report_Type()
        {
            var report = new ReportInfo
            {
                Schema = "public",
                Name = "sales",
                Type = "Table"
            };

            var act = () => PostgreReportSqlBuilder.BuildExecuteReportSql(report);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Некорректный тип отчета.*");
        }

        [Fact]
        public void BuildExecuteReportSql_Rejects_Null_Identifier()
        {
            var report = new ReportInfo
            {
                Schema = null!,
                Name = "sales",
                Type = "View"
            };

            var act = () => PostgreReportSqlBuilder.BuildExecuteReportSql(report);

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("bad\0name")]
        public void BuildExecuteReportSql_Rejects_Invalid_Identifiers(string identifier)
        {
            var report = new ReportInfo
            {
                Schema = identifier,
                Name = "sales",
                Type = "View"
            };

            var act = () => PostgreReportSqlBuilder.BuildExecuteReportSql(report);

            act.Should().Throw<ArgumentException>();
        }
    }
}
