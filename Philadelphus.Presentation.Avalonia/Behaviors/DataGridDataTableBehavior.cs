using System;
using System.Collections.Generic;
using System.Data;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Data;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Привязывает <see cref="DataGrid"/> к <see cref="DataTable"/>: строит текстовые колонки из
    /// <see cref="DataTable.Columns"/> и подставляет <see cref="DataTable.DefaultView"/> как источник строк.
    /// В отличие от WPF, Avalonia DataGrid не умеет авто-генерацию колонок из DataTable.
    /// </summary>
    public class DataGridDataTableBehavior
    {
        private DataGridDataTableBehavior()
        {
        }

        /// <summary>Источник-таблица для DataGrid.</summary>
        public static readonly AttachedProperty<DataTable?> SourceProperty =
            AvaloniaProperty.RegisterAttached<DataGridDataTableBehavior, DataGrid, DataTable?>("Source");

        public static DataTable? GetSource(DataGrid o) => o.GetValue(SourceProperty);
        public static void SetSource(DataGrid o, DataTable? value) => o.SetValue(SourceProperty, value);

        static DataGridDataTableBehavior()
        {
            SourceProperty.Changed.AddClassHandler<DataGrid>((grid, e) => Rebuild(grid, e.NewValue as DataTable));
        }

        private static void Rebuild(DataGrid dataGrid, DataTable? table)
        {
            dataGrid.Columns.Clear();

            if (table == null)
            {
                dataGrid.ItemsSource = null;
                return;
            }

            foreach (DataColumn column in table.Columns)
            {
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = column.ColumnName,
                    Binding = new Binding($"[{column.ColumnName}]"),
                    IsReadOnly = true,
                });
            }

            // Строки приводим к словарям: индексатор [ключ] у IDictionary Avalonia понимает,
            // а к DataRowView (значения через ICustomTypeDescriptor) привязка не работает.
            var rows = new List<Dictionary<string, object?>>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                var cells = new Dictionary<string, object?>(table.Columns.Count, StringComparer.Ordinal);
                foreach (DataColumn column in table.Columns)
                {
                    var value = row[column];
                    cells[column.ColumnName] = value == DBNull.Value ? null : value;
                }

                rows.Add(cells);
            }

            dataGrid.ItemsSource = rows;
        }
    }
}
