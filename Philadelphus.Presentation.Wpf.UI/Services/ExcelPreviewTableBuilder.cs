using Philadelphus.Infrastructure.ImportExport.Excel;
using System.Data;
using System.Linq;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    internal static class ExcelPreviewTableBuilder
    {
        internal static DataTable Build(ExcelPreviewTable preview)
        {
            var table = new DataTable();

            foreach (var header in preview.Headers)
            {
                var columnName = string.IsNullOrWhiteSpace(header) ? "Колонка" : header;
                var uniqueColumnName = columnName;
                var suffix = 1;

                while (table.Columns.Contains(uniqueColumnName))
                {
                    suffix++;
                    uniqueColumnName = $"{columnName} ({suffix})";
                }

                table.Columns.Add(uniqueColumnName);
            }

            foreach (var row in preview.Rows)
            {
                var rowValues = row.Cast<object>().ToArray();
                table.Rows.Add(rowValues);
            }

            return table;
        }
    }
}
