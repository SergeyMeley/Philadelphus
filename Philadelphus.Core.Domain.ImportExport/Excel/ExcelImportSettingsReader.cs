using ClosedXML.Excel;
using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportSettingsReader : IExcelImportSettingsReader
    {
        private readonly IExcelImportSourceReader _sourceReader;

        public ExcelImportSettingsReader(IExcelImportSourceReader sourceReader)
        {
            _sourceReader = sourceReader;
        }

        public ExcelImportSettingsDocument Read(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            return Read(workbook);
        }

        private ExcelImportSettingsDocument Read(XLWorkbook workbook)
        {
            var settingsWorksheet = workbook.Worksheets
                .FirstOrDefault(worksheet => _sourceReader.IsSettingsWorksheet(worksheet.Name));

            if (settingsWorksheet == null)
                return new ExcelImportSettingsDocument();

            var usedRange = settingsWorksheet.RangeUsed();
            if (usedRange == null)
                return new ExcelImportSettingsDocument();

            var rows = usedRange.RowsUsed().ToList();
            if (rows.Count <= 1)
                return new ExcelImportSettingsDocument();

            var headers = BuildHeaderMap(rows[0]);
            var document = new ExcelImportSettingsDocument();

            foreach (var row in rows.Skip(1))
            {
                if (IsEmptyRow(row))
                    continue;

                var item = new ExcelImportSettingsRowDto
                {
                    RuleType = GetString(row, headers, "RuleType"),
                    SourceName = GetString(row, headers, "SourceName"),
                    ParentSourceName = GetString(row, headers, "ParentSourceName"),
                    ParentKeyColumnName = GetString(row, headers, "ParentKeyColumnName"),
                    ChildKeyColumnName = GetString(row, headers, "ChildKeyColumnName"),
                    ColumnIndex = GetNullableInt(row, headers, "ColumnIndex"),
                    HeaderName = GetString(row, headers, "HeaderName"),
                    Role = GetEnum<ExcelImportColumnRole>(row, headers, "Role"),
                    DefinitionScope = GetEnum<ExcelImportDefinitionScope>(row, headers, "DefinitionScope"),
                    ValueMode = GetEnum<ExcelImportValueMode>(row, headers, "ValueMode"),
                    DataTypeNodeName = GetString(row, headers, "DataTypeNodeName"),
                    IsCollectionValue = GetNullableBool(row, headers, "IsCollectionValue"),
                    Visibility = GetEnum<VisibilityScope>(row, headers, "Visibility"),
                    Override = GetEnum<OverrideType>(row, headers, "Override"),
                    Description = GetString(row, headers, "Description"),
                    DefaultValue = GetString(row, headers, "DefaultValue")
                };

                switch ((item.RuleType ?? string.Empty).Trim().ToLowerInvariant())
                {
                    case "workbookdefault":
                        document.WorkbookDefaults.Add(item);
                        break;
                    case "worksheetdefault":
                        document.WorksheetDefaults.Add(item);
                        break;
                    case "columnrule":
                        document.ColumnRules.Add(item);
                        break;
                }
            }

            return document;
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLRangeRow headerRow)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var header = cell.GetString().Trim();
                if (string.IsNullOrWhiteSpace(header) || result.ContainsKey(header))
                    continue;

                result[header] = cell.Address.ColumnNumber;
            }

            return result;
        }

        private static bool IsEmptyRow(IXLRangeRow row)
        {
            return row.CellsUsed().Any() == false
                || row.CellsUsed().All(cell => string.IsNullOrWhiteSpace(cell.GetString()));
        }

        private static string GetString(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, string headerName)
        {
            return headers.TryGetValue(headerName, out var columnNumber)
                ? row.Cell(columnNumber).GetString().Trim()
                : string.Empty;
        }

        private static int? GetNullableInt(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, string headerName)
        {
            var value = GetString(row, headers, headerName);
            return int.TryParse(value, out var parsed) ? parsed : null;
        }

        private static bool? GetNullableBool(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, string headerName)
        {
            var value = GetString(row, headers, headerName);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (bool.TryParse(value, out var parsed))
                return parsed;

            if (value == "1")
                return true;

            if (value == "0")
                return false;

            return null;
        }

        private static TEnum? GetEnum<TEnum>(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, string headerName)
            where TEnum : struct, Enum
        {
            var value = GetString(row, headers, headerName);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Enum.TryParse<TEnum>(value, true, out var parsed))
                return parsed;

            foreach (var candidate in Enum.GetValues<TEnum>())
            {
                var member = typeof(TEnum).GetMember(candidate.ToString()).FirstOrDefault();
                var display = member?.GetCustomAttribute<DisplayAttribute>();
                if (string.Equals(display?.Name, value, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return null;
        }
    }
}
