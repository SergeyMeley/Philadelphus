using System.Collections.Generic;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public interface IExcelDataTypeDetector
    {
        string DetermineBestDataType(IEnumerable<string?> values);

        bool IsValueCompatibleWithDataType(string value, string dataTypeNodeName);
    }
}
