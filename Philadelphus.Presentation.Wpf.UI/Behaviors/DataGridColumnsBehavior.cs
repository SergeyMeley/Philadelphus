using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Converters;
using Philadelphus.Presentation.Wpf.UI.Models.Tables;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Attached behavior, который строит WPF DataGrid-колонки из presentation-моделей таблицы наследников.
    /// </summary>
    /// <remarks>
    /// Генерация колонок вынесена из code-behind: ViewModel отдает чистые дескрипторы колонок,
    /// а behavior отвечает только за WPF-представление этих дескрипторов.
    /// </remarks>
    public static class DataGridColumnsBehavior
    {
        private static readonly StateToColorConverter StateToColorConverter = new StateToColorConverter();
        private static readonly EnumDisplayAttributeConverter EnumDisplayAttributeConverter = new EnumDisplayAttributeConverter();

        /// <summary>
        /// Источник дескрипторов колонок для динамического DataGrid.
        /// </summary>
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

        /// <summary>
        /// Пересоздает DataGrid-колонки при изменении набора дескрипторов.
        /// </summary>
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

        /// <summary>
        /// Создает узкую цветовую колонку состояния по аналогии с TreeView и таблицей атрибутов.
        /// </summary>
        private static DataGridColumn CreateStateColumn(ChildCollectionTableColumn column)
        {
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(
                Control.BackgroundProperty,
                new Binding($"[{column.BindingKey}]") { Converter = StateToColorConverter }));
            cellStyle.Setters.Add(new Setter(Control.ForegroundProperty, System.Windows.Media.Brushes.Black));
            cellStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            cellStyle.Setters.Add(new Setter(
                FrameworkElement.ToolTipProperty,
                new Binding($"[{column.BindingKey}]")
                {
                    Converter = EnumDisplayAttributeConverter,
                    ConverterParameter = "Description",
                }));

            return new DataGridTemplateColumn
            {
                Header = CreateStateColumnHeader(column),
                IsReadOnly = true,
                Width = new DataGridLength(7),
                MinWidth = 7,
                CellStyle = cellStyle,
                CellTemplate = CreateEmptyCellTemplate(),
            };
        }

        private static TextBlock CreateStateColumnHeader(ChildCollectionTableColumn column)
        {
            return new TextBlock
            {
                Text = string.Empty,
                ToolTip = column.HeaderToolTip ?? column.Header,
                MinWidth = 7,
            };
        }

        /// <summary>
        /// Создает колонку выбора значения для редактируемых одиночных атрибутов.
        /// </summary>
        private static DataGridComboBoxColumn CreateComboBoxColumn(ChildCollectionTableColumn column)
        {
            var selectedItemBinding = new Binding($"[{column.BindingKey}]")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            var itemsSourceBinding = new Binding($"ValueOptions[{column.BindingKey}]");

            return new DataGridComboBoxColumn
            {
                Header = CreateColumnHeader(column),
                SelectedItemBinding = selectedItemBinding,
                DisplayMemberPath = "Name",
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = column.IsAttribute ? 120 : 80,
                ElementStyle = CreateComboBoxStyle(itemsSourceBinding),
                EditingElementStyle = CreateComboBoxStyle(itemsSourceBinding),
            };
        }

        /// <summary>
        /// Создает текстовую колонку для readonly и простых редактируемых значений.
        /// </summary>
        private static DataGridTextColumn CreateTextColumn(ChildCollectionTableColumn column)
        {
            var binding = new Binding($"[{column.BindingKey}]")
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
                Header = CreateColumnHeader(column),
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

        private static DataTemplate CreateEmptyCellTemplate()
        {
            var template = new DataTemplate();
            template.VisualTree = new FrameworkElementFactory(typeof(Border));
            return template;
        }

        private static object CreateColumnHeader(ChildCollectionTableColumn column)
        {
            if (string.IsNullOrWhiteSpace(column.HeaderToolTip))
            {
                return column.Header;
            }

            return new TextBlock
            {
                Text = column.Header,
                ToolTip = column.HeaderToolTip,
            };
        }
    }
}
