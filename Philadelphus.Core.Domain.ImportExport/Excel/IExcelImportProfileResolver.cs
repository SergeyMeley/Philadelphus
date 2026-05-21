namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public interface IExcelImportProfileResolver
    {
        ExcelImportProfile Resolve(string filePath, ExcelImportSourceSelection selection, ExcelImportProfile detectedProfile);
    }
}
