namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public interface IExcelImportSchemaTemplateStorage
    {
        void Save(string filePath, ExcelImportSchema schema);

        ExcelImportSchema Load(string filePath);
    }
}
