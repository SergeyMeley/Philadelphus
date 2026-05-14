namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public interface IExcelImportSettingsReader
    {
        ExcelImportSettingsDocument Read(string filePath);
    }
}
