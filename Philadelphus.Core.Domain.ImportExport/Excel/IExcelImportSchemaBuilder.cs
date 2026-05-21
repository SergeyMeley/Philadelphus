namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public interface IExcelImportSchemaBuilder
    {
        ExcelImportSchema CreateDraftSchema(string filePath, string rootName);
    }
}
