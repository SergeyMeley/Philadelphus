using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System.Text;

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

        public static string BuildRecreateMaterializedViewSql(ReportInfo report, string creationSql)
        {
            ArgumentNullException.ThrowIfNull(report);

            if (report.Type != "MaterializedView")
            {
                throw new ArgumentException("Invalid report type.", nameof(report));
            }

            var reportName = QuoteIdentifier(report.Name, nameof(report.Name));
            var selectSql = ExtractCreateTableAsSelect(creationSql, report.Name);
            return $"CREATE TABLE {reportName} AS {selectSql}";
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

        private static string ExtractCreateTableAsSelect(string creationSql, string expectedTableName)
        {
            if (string.IsNullOrWhiteSpace(creationSql))
            {
                throw new InvalidOperationException("creation_sql must not be empty.");
            }

            var sql = TrimSingleTrailingSemicolon(creationSql.Trim());
            if (sql.IndexOf(';') >= 0)
            {
                throw new InvalidOperationException("creation_sql must contain exactly one statement.");
            }

            var index = 0;
            ReadKeyword(sql, ref index, "CREATE");
            ReadKeyword(sql, ref index, "TABLE");

            if (TryReadKeyword(sql, ref index, "IF"))
            {
                ReadKeyword(sql, ref index, "NOT");
                ReadKeyword(sql, ref index, "EXISTS");
            }

            var targetTableName = ReadTableName(sql, ref index);
            if (!StringComparer.Ordinal.Equals(targetTableName, expectedTableName))
            {
                throw new InvalidOperationException("creation_sql target table does not match report name.");
            }

            ReadKeyword(sql, ref index, "AS");
            SkipWhitespace(sql, ref index);

            if (!IsKeywordAt(sql, index, "SELECT") && !IsKeywordAt(sql, index, "WITH"))
            {
                throw new InvalidOperationException("creation_sql must create the table from a SELECT query.");
            }

            return sql[index..].Trim();
        }

        private static string TrimSingleTrailingSemicolon(string sql)
        {
            return sql.EndsWith(';') ? sql[..^1].TrimEnd() : sql;
        }

        private static string ReadTableName(string sql, ref int index)
        {
            var tableName = ReadIdentifier(sql, ref index);
            SkipWhitespace(sql, ref index);

            if (index < sql.Length && sql[index] == '.')
            {
                index++;
                tableName = ReadIdentifier(sql, ref index);
            }

            return tableName;
        }

        private static string ReadIdentifier(string sql, ref int index)
        {
            SkipWhitespace(sql, ref index);
            if (index >= sql.Length)
            {
                throw new InvalidOperationException("Expected SQL identifier.");
            }

            return sql[index] switch
            {
                '"' => ReadQuotedIdentifier(sql, ref index, '"'),
                '[' => ReadBracketedIdentifier(sql, ref index),
                '`' => ReadQuotedIdentifier(sql, ref index, '`'),
                _ => ReadBareIdentifier(sql, ref index)
            };
        }

        private static string ReadQuotedIdentifier(string sql, ref int index, char quote)
        {
            index++;
            var value = new StringBuilder();

            while (index < sql.Length)
            {
                var current = sql[index++];
                if (current == quote)
                {
                    if (quote == '"' && index < sql.Length && sql[index] == '"')
                    {
                        value.Append('"');
                        index++;
                        continue;
                    }

                    return value.ToString();
                }

                value.Append(current);
            }

            throw new InvalidOperationException("Unterminated quoted identifier.");
        }

        private static string ReadBracketedIdentifier(string sql, ref int index)
        {
            index++;
            var start = index;
            while (index < sql.Length && sql[index] != ']')
            {
                index++;
            }

            if (index >= sql.Length)
            {
                throw new InvalidOperationException("Unterminated bracketed identifier.");
            }

            var value = sql[start..index];
            index++;
            return value;
        }

        private static string ReadBareIdentifier(string sql, ref int index)
        {
            var start = index;
            while (index < sql.Length && !char.IsWhiteSpace(sql[index]) && sql[index] != '.' && sql[index] != '(')
            {
                index++;
            }

            if (start == index)
            {
                throw new InvalidOperationException("Expected SQL identifier.");
            }

            return sql[start..index];
        }

        private static void ReadKeyword(string sql, ref int index, string keyword)
        {
            if (!TryReadKeyword(sql, ref index, keyword))
            {
                throw new InvalidOperationException($"Expected {keyword} keyword.");
            }
        }

        private static bool TryReadKeyword(string sql, ref int index, string keyword)
        {
            var originalIndex = index;
            SkipWhitespace(sql, ref index);

            if (!IsKeywordAt(sql, index, keyword))
            {
                index = originalIndex;
                return false;
            }

            index += keyword.Length;
            return true;
        }

        private static bool IsKeywordAt(string sql, int index, string keyword)
        {
            if (index + keyword.Length > sql.Length)
            {
                return false;
            }

            if (!string.Equals(sql.Substring(index, keyword.Length), keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var end = index + keyword.Length;
            return end == sql.Length || !IsIdentifierChar(sql[end]);
        }

        private static bool IsIdentifierChar(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        private static void SkipWhitespace(string sql, ref int index)
        {
            while (index < sql.Length && char.IsWhiteSpace(sql[index]))
            {
                index++;
            }
        }
    }
}
