using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportSourceReader : IExcelImportSourceReader
    {
        public string SettingsWorksheetName => "#ImportSettings";

        public bool IsSettingsWorksheet(string worksheetName)
        {
            return string.Equals(
                worksheetName?.Trim(),
                SettingsWorksheetName,
                StringComparison.OrdinalIgnoreCase);
        }

        public IReadOnlyList<IXLWorksheet> GetImportableWorksheets(XLWorkbook workbook)
        {
            return workbook.Worksheets
                .Where(worksheet => IsSettingsWorksheet(worksheet.Name) == false)
                .ToList();
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

            var namedRange = workbook.DefinedNames
                .Concat(workbook.Worksheets.SelectMany(x => x.DefinedNames))
                .FirstOrDefault(x => string.Equals(x.Name, selection.SourceName, StringComparison.OrdinalIgnoreCase));

            return namedRange?.Ranges.FirstOrDefault()?.RangeAddress.AsRange();
        }
    }
}
