using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Converters;
using Philadelphus.Presentation.Wpf.UI.Models.Tables;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    public static class DataGridColumnsBehavior
    {
        private static readonly StateToColorConverter StateToColorConverter = new StateToColorConverter();

        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.RegisterAttached(
                "ColumnsSource",
                typeof(IEnumerable),
                typeof(DataGridColumnsBehavior),
                new PropertyMetadata(null, OnColumnsSourceChanged));

        public static IEnumerable? GetColumnsSource(DependencyObject obj)
        {
            return (IEnumerable?)obj.GetValue(ColumnsSourceProperty);
        }

        public static void SetColumnsSource(DependencyObject obj, IEnumerable? value)
        {
            obj.SetValue(ColumnsSourceProperty, value);
        }

        private static void OnColumnsSourceChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not DataGrid dataGrid)
            {
                return;
            }

            RebuildColumns(dataGrid, e.NewValue as IEnumerable);
        }

        private static void RebuildColumns(DataGrid dataGrid, IEnumerable? columnsSource)
        {
            dataGrid.Columns.Clear();

            if (columnsSource == null)
            {
                return;
            }

            foreach (var column in columnsSource.OfType<ChildCollectionTableColumn>().OrderBy(x => x.Order))
            {
                dataGrid.Columns.Add(column.Key == nameof(IMainEntityModel.State)
                    ? CreateStateColumn(column)
                    : column.HasValueOptions
                    ? CreateComboBoxColumn(column)
                    : CreateTextColumn(column));
            }
        }

        private static DataGridTextColumn CreateStateColumn(ChildCollectionTableColumn column)
        {
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(
                Control.BackgroundProperty,
                new Binding($"[{column.Key}]") { Converter = StateToColorConverter }));
            cellStyle.Setters.Add(new Setter(Control.ForegroundProperty, System.Windows.Media.Brushes.Black));

            return new DataGridTextColumn
            {
                Header = column.Header,
                Binding = new Binding($"[{column.Key}]") { Mode = BindingMode.OneWay },
                IsReadOnly = true,
                Width = DataGridLength.Auto,
                MinWidth = 90,
                CellStyle = cellStyle,
            };
        }

        private static DataGridComboBoxColumn CreateComboBoxColumn(ChildCollectionTableColumn column)
        {
            var selectedItemBinding = new Binding($"[{column.Key}]")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            var itemsSourceBinding = new Binding($"ValueOptions[{column.Key}]");

            return new DataGridComboBoxColumn
            {
                Header = column.Header,
                SelectedItemBinding = selectedItemBinding,
                DisplayMemberPath = "Name",
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = column.IsAttribute ? 120 : 80,
                ElementStyle = CreateComboBoxStyle(itemsSourceBinding),
                EditingElementStyle = CreateComboBoxStyle(itemsSourceBinding),
            };
        }

        private static DataGridTextColumn CreateTextColumn(ChildCollectionTableColumn column)
        {
            var binding = new Binding($"[{column.Key}]")
            {
                Mode = column.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            };

            if (column.Key.EndsWith("At", StringComparison.OrdinalIgnoreCase))
            {
                binding.StringFormat = "yyyy-MM-dd HH:mm";
            }

            return new DataGridTextColumn
            {
                Header = column.Header,
                Binding = binding,
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = column.IsAttribute ? 120 : 80,
            };
        }

        private static Style CreateComboBoxStyle(Binding itemsSourceBinding)
        {
            var style = new Style(typeof(ComboBox));
            style.Setters.Add(new Setter(ItemsControl.ItemsSourceProperty, itemsSourceBinding));
            return style;
        }
    }
}
