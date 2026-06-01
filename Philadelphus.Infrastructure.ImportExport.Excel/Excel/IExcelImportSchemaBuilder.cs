namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public interface IExcelImportSchemaBuilder
    {
        ExcelImportSchema CreateDraftSchema(string filePath, string rootName);
    }
}
