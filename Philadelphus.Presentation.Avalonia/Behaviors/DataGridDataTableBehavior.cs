using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Data;
using global::Avalonia.Data.Converters;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Привязывает <see cref="DataGrid"/> к <see cref="DataTable"/> или <see cref="DataView"/>:
    /// строит текстовые колонки из столбцов и подставляет строки. В отличие от WPF, Avalonia DataGrid
    /// не умеет авто-генерацию колонок из DataTable/DataView.
    /// </summary>
    public class DataGridDataTableBehavior
    {
        private DataGridDataTableBehavior()
        {
        }

        /// <summary>Источник для DataGrid: <see cref="DataTable"/> или <see cref="DataView"/>.</summary>
        public static readonly AttachedProperty<object?> SourceProperty =
            AvaloniaProperty.RegisterAttached<DataGridDataTableBehavior, DataGrid, object?>("Source");

        public static object? GetSource(DataGrid o) => o.GetValue(SourceProperty);
        public static void SetSource(DataGrid o, object? value) => o.SetValue(SourceProperty, value);

        static DataGridDataTableBehavior()
        {
            SourceProperty.Changed.AddClassHandler<DataGrid>((grid, e) => Rebuild(grid, e.NewValue));
        }

        private static void Rebuild(DataGrid dataGrid, object? source)
        {
            dataGrid.Columns.Clear();

            var view = source switch
            {
                DataView dataView => dataView,
                DataTable table => table.DefaultView,
                _ => null,
            };

            if (view == null)
            {
                dataGrid.ItemsSource = null;
                return;
            }

            var columns = view.Table.Columns;
            foreach (DataColumn column in columns)
            {
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = column.ColumnName,
                    // Привязка через конвертер с именем колонки в параметре, а НЕ по строковому пути
                    // [имя]: имена колонок Excel могут содержать пробелы/запятые/скобки и ломать парсер пути.
                    Binding = new Binding
                    {
                        Converter = CellValueConverter.Instance,
                        ConverterParameter = column.ColumnName,
                    },
                    IsReadOnly = true,
                });
            }

            // Строки приводим к словарям (значения по ключу-имени колонки).
            var rows = new List<Dictionary<string, object?>>(view.Count);
            foreach (DataRowView rowView in view)
            {
                var cells = new Dictionary<string, object?>(columns.Count, StringComparer.Ordinal);
                foreach (DataColumn column in columns)
                {
                    var value = rowView[column.ColumnName];
                    cells[column.ColumnName] = value == DBNull.Value ? null : value;
                }

                rows.Add(cells);
            }

            dataGrid.ItemsSource = rows;
        }

        /// <summary>Достаёт значение ячейки из словаря строки по имени колонки (ConverterParameter).</summary>
        private sealed class CellValueConverter : IValueConverter
        {
            public static readonly CellValueConverter Instance = new();

            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (value is IReadOnlyDictionary<string, object?> cells
                    && parameter is string key
                    && cells.TryGetValue(key, out var cellValue))
                {
                    return cellValue;
                }

                return null;
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
                => throw new NotSupportedException();
        }
    }
}
