using Philadelphus.Infrastructure.Persistence.Entities.Reports;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL
{
    internal static class PostgreReportSqlBuilder
    {
        public static PostgreReportSql BuildExecuteReportSql(ReportInfo report)
        {
            ArgumentNullException.ThrowIfNull(report);

            var qualifiedName = BuildQualifiedName(report.Schema, report.Name);

            return report.Type switch
            {
                "Function" => BuildFunctionSql(report, qualifiedName),
                "View" or "MaterializedView" => new PostgreReportSql($"SELECT * FROM {qualifiedName}", []),
                _ => throw new ArgumentException("Некорректный тип отчета.", nameof(report))
            };
        }

        public static string BuildRefreshMaterializedViewSql(ReportInfo report, bool concurrently)
        {
            ArgumentNullException.ThrowIfNull(report);

            var concurrentlyClause = concurrently ? "CONCURRENTLY " : string.Empty;
            return $"REFRESH MATERIALIZED VIEW {concurrentlyClause}{BuildQualifiedName(report.Schema, report.Name)}";
        }

        private static PostgreReportSql BuildFunctionSql(ReportInfo report, string qualifiedName)
        {
            var parameters = report.Parameters ?? [];
            var parameterNames = new List<string>(parameters.Count);

            for (var index = 0; index < parameters.Count; index++)
            {
                parameterNames.Add($"@p{index}");
            }

            var parameterList = string.Join(", ", parameterNames);
            return new PostgreReportSql($"SELECT * FROM {qualifiedName}({parameterList})", parameterNames);
        }

        private static string BuildQualifiedName(string schema, string name)
        {
            return $"{QuoteIdentifier(schema, nameof(schema))}.{QuoteIdentifier(name, nameof(name))}";
        }

        private static string QuoteIdentifier(string identifier, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Идентификатор не должен быть пустым.", parameterName);
            }

            if (identifier.IndexOf('\0') >= 0)
            {
                throw new ArgumentException("Идентификатор не должен содержать null-символ.", parameterName);
            }

            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }
    }

    internal sealed record PostgreReportSql(string CommandText, IReadOnlyList<string> ParameterNames);
}
