using ClosedXML.Excel;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public interface IExcelImportInheritanceResolver
    {
        ExcelImportInheritanceInfo Resolve(IXLRange range, ExcelImportColumnProfile profile);
    }
}
