using ClosedXML.Excel;
using System;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportInheritanceResolver : IExcelImportInheritanceResolver
    {
        public ExcelImportInheritanceInfo Resolve(IXLRange range, ExcelImportColumnProfile profile)
        {
            var distinctValues = range.RowsUsed()
                .Skip(1)
                .Select(row => row.Cell(profile.ColumnIndex).GetString().Trim())
                .Where(value => string.IsNullOrWhiteSpace(value) == false)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var resolvedParentValue = string.IsNullOrWhiteSpace(profile.DefaultValue) == false
                ? profile.DefaultValue.Trim()
                : distinctValues.Count == 1
                    ? distinctValues[0]
                    : null;

            return new ExcelImportInheritanceInfo
            {
                DistinctNonEmptyValues = distinctValues,
                ResolvedParentValue = resolvedParentValue
            };
        }
    }
}
