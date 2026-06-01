namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public interface IExcelImportSettingsReader
    {
        ExcelImportSettingsDocument Read(string filePath);
    }
}
