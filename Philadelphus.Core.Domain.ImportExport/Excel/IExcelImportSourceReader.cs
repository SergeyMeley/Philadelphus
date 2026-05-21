using ClosedXML.Excel;
using System.Collections.Generic;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public interface IExcelImportSourceReader
    {
        string SettingsWorksheetName { get; }

        bool IsSettingsWorksheet(string worksheetName);

        IReadOnlyList<IXLWorksheet> GetImportableWorksheets(XLWorkbook workbook);

        IReadOnlyList<IXLTable> GetImportableTables(XLWorkbook workbook);

        IReadOnlyList<IXLDefinedName> GetImportableNamedRanges(XLWorkbook workbook);

        IReadOnlyList<ExcelImportSourceSelection> GetDefaultSourceSelections(XLWorkbook workbook);

        IXLRange? ResolveRange(XLWorkbook workbook, ExcelImportSourceSelection selection);
    }
}
