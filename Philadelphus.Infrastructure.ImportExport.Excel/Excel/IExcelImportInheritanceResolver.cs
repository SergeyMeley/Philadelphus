using ClosedXML.Excel;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public interface IExcelImportInheritanceResolver
    {
        ExcelImportInheritanceInfo Resolve(IXLRange range, ExcelImportColumnProfile profile);
    }
}
