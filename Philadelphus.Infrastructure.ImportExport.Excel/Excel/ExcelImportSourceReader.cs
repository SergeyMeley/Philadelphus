using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public class ExcelImportSourceReader : IExcelImportSourceReader
    {
        public string SettingsWorksheetName => "#ImportSettings";

        public bool IsSettingsWorksheet(string worksheetName)
        {
            // Этот лист не импортируется как данные. Он только накладывает правила профиля:
            // наследование, типы данных, роли колонок и relation keys.
            var normalizedName = (worksheetName ?? string.Empty).Trim();
            return string.Equals(normalizedName, SettingsWorksheetName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedName, "#НастройкиИмпорта", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedName, "Настройки импорта", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedName, "Настройки", StringComparison.OrdinalIgnoreCase);
        }

        public IReadOnlyList<IXLWorksheet> GetImportableWorksheets(XLWorkbook workbook)
        {
            var worksheets = workbook.Worksheets
                .Where(worksheet => IsSettingsWorksheet(worksheet.Name) == false)
                .ToList();
            ExcelImportLimits.ValidateSourceCount(worksheets.Count);
            return worksheets;
        }

        public IReadOnlyList<IXLTable> GetImportableTables(XLWorkbook workbook)
        {
            var tables = GetImportableWorksheets(workbook)
                .SelectMany(worksheet => worksheet.Tables)
                .GroupBy(table => table.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
            ExcelImportLimits.ValidateSourceCount(tables.Count);
            return tables;
        }

        public IReadOnlyList<ExcelImportSourceSelection> GetDefaultSourceSelections(XLWorkbook workbook)
        {
            var tables = GetImportableTables(workbook);
            var explicitWorksheetNames = tables
                .Select(table => table.Worksheet.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var namedRange in GetImportableNamedRanges(workbook))
            {
                var range = GetSingleRange(namedRange);
                if (range == null)
                    continue;

                explicitWorksheetNames.Add(range.Worksheet.Name);
            }

            var result = new List<ExcelImportSourceSelection>();

            // Если лист содержит явные Excel Table или NamedRange, источником считается этот диапазон,
            // а не весь used range листа: иначе грязный лист снова попадет в импорт как одна каша.
            result.AddRange(tables.Select(table => new ExcelImportSourceSelection
            {
                SourceName = table.Name,
                SourceType = ExcelPreviewSourceType.Table
            }));

            result.AddRange(GetImportableNamedRanges(workbook).Select(namedRange => new ExcelImportSourceSelection
            {
                SourceName = namedRange.Name,
                SourceType = ExcelPreviewSourceType.NamedRange
            }));

            result.AddRange(GetImportableWorksheets(workbook)
                .Where(worksheet => explicitWorksheetNames.Contains(worksheet.Name) == false)
                .Select(worksheet => new ExcelImportSourceSelection
                {
                    SourceName = worksheet.Name,
                    SourceType = ExcelPreviewSourceType.Worksheet
                }));

            ExcelImportLimits.ValidateSourceCount(result.Count);
            return result;
        }

        public IXLRange? ResolveRange(XLWorkbook workbook, ExcelImportSourceSelection selection)
        {
            if (selection.SourceType == ExcelPreviewSourceType.Worksheet)
            {
                var worksheet = workbook.Worksheets.FirstOrDefault(x => string.Equals(x.Name, selection.SourceName, StringComparison.OrdinalIgnoreCase));
                if (worksheet == null || IsSettingsWorksheet(worksheet.Name))
                    return null;

                return worksheet.RangeUsed();
            }

            if (selection.SourceType == ExcelPreviewSourceType.Table)
            {
                var table = GetImportableTables(workbook)
                    .FirstOrDefault(x => string.Equals(x.Name, selection.SourceName, StringComparison.OrdinalIgnoreCase));

                return table;
            }

            var namedRange = GetImportableNamedRanges(workbook)
                .FirstOrDefault(x => string.Equals(x.Name, selection.SourceName, StringComparison.OrdinalIgnoreCase));

            return GetSingleRange(namedRange);
        }

        public IReadOnlyList<IXLDefinedName> GetImportableNamedRanges(XLWorkbook workbook)
        {
            var workbookRanges = workbook.DefinedNames.ToList();
            var worksheetRanges = GetImportableWorksheets(workbook)
                .SelectMany(worksheet => worksheet.DefinedNames)
                .ToList();

            var ranges = workbookRanges
                .Concat(worksheetRanges)
                .Where(range => range.IsValid && IsUserNamedRange(range.Name) && IsTabularNamedRange(range))
                .GroupBy(range => range.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
            ExcelImportLimits.ValidateSourceCount(ranges.Count);
            return ranges;
        }

        private static bool IsUserNamedRange(string name)
        {
            var normalized = (name ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(normalized) == false
                && normalized.StartsWith("_xlnm.", StringComparison.OrdinalIgnoreCase) == false
                && string.Equals(normalized, "Print_Area", StringComparison.OrdinalIgnoreCase) == false
                && string.Equals(normalized, "Print_Titles", StringComparison.OrdinalIgnoreCase) == false;
        }

        private static bool IsTabularNamedRange(IXLDefinedName definedName)
        {
            var range = GetSingleRange(definedName);
            var firstRow = range?.FirstRowUsed();
            var lastRow = range?.LastRowUsed();
            var firstColumn = range?.FirstColumnUsed();

            // NamedRange используем как источник только когда там есть header row и хотя бы одна строка данных.
            // Иначе одиночные именованные ячейки в грязных книгах ошибочно вытесняют нормальный импорт листа.
            return firstRow != null
                && lastRow != null
                && firstColumn != null
                && lastRow.RowNumber() > firstRow.RowNumber();
        }

        private static IXLRange? GetSingleRange(IXLDefinedName? definedName)
        {
            return definedName?.Ranges.FirstOrDefault()?.RangeAddress.AsRange();
        }
    }
}
