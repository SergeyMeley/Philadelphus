using Philadelphus.Infrastructure.Persistence.Entities.Reports;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite
{
    internal static class SqliteReportSqlBuilder
    {
        public const string AttachDatabasePathParameterName = "@path";

        public static string BuildExecuteReportSql(ReportInfo report)
        {
            ArgumentNullException.ThrowIfNull(report);

            return report.Type switch
            {
                "View" or "MaterializedView" => $"SELECT * FROM {QuoteIdentifier(report.Name, nameof(report.Name))}",
                _ => throw new ArgumentException("Invalid report type.", nameof(report))
            };
        }

        public static string BuildDropTableSql(ReportInfo report)
        {
            ArgumentNullException.ThrowIfNull(report);

            return $"DROP TABLE IF EXISTS {QuoteIdentifier(report.Name, nameof(report.Name))}";
        }

        public static string BuildAttachDatabaseSql(string alias)
        {
            return $"ATTACH DATABASE {AttachDatabasePathParameterName} AS {QuoteIdentifier(alias, nameof(alias))};";
        }

        private static string QuoteIdentifier(string identifier, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Identifier must not be empty.", parameterName);
            }

            if (identifier.IndexOf('\0') >= 0)
            {
                throw new ArgumentException("Identifier must not contain null characters.", parameterName);
            }

            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }
    }
}
