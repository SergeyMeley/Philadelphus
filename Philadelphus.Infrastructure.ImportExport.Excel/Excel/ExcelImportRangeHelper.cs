using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    /// <summary>
    /// Общие правила чтения строк Excel для импорта.
    /// Важно: первый used row всегда считается строкой заголовков; DataStartRowOffset показывает,
    /// сколько первых used rows надо пропустить перед чтением реальных данных.
    /// </summary>
    public static class ExcelImportRangeHelper
    {
        public static int NormalizeDataStartRowOffset(int dataStartRowOffset)
        {
            // Минимальное смещение 1 защищает header row от попадания в данные даже для старых профилей.
            return Math.Max(1, dataStartRowOffset);
        }

        public static List<IXLRangeRow> GetDataRows(IXLRange range, int dataStartRowOffset)
        {
            ExcelImportLimits.ValidateRange(range, string.Empty);

            return range.RowsUsed()
                .Skip(NormalizeDataStartRowOffset(dataStartRowOffset))
                .ToList();
        }

        public static string GetCellText(IXLRangeRow row, int columnIndex)
        {
            // GetFormattedString сохраняет пользовательский вид чисел/дат лучше, чем GetString().
            return row.Cell(columnIndex).GetFormattedString().Trim();
        }

        public static Dictionary<int, ExcelImportColumnRole> DetectMarkerRoles(IReadOnlyList<IXLRangeRow> rows, int columnCount)
        {
            var result = new Dictionary<int, ExcelImportColumnRole>();
            if (rows.Count < 2)
                return result;

            // Поддерживаем формат: 1-я строка - человекочитаемые заголовки, 2-я строка - #Name/#Описание/#Sequence.
            // Если маркеры стоят прямо в 1-й строке, их разбирает ExcelImportColumnRoleHelper.DetectRole().
            var markerRow = rows[1];
            for (var columnIndex = 1; columnIndex <= columnCount; columnIndex++)
            {
                var marker = GetCellText(markerRow, columnIndex);
                if (ExcelImportColumnRoleHelper.TryDetectMarkerRole(marker, out var role))
                {
                    result[columnIndex] = role;
                }
            }

            return result;
        }
    }
}
