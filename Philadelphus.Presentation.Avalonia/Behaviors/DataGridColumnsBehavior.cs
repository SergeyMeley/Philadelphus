using System;
using System.Collections;
using System.Linq;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Data;
using global::Avalonia.Layout;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.Avalonia.Converters;
using Philadelphus.Presentation.Models.Tables;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Attached-поведение: строит колонки <see cref="DataGrid"/> из presentation-дескрипторов
    /// <see cref="ChildCollectionTableColumn"/>. Avalonia-аналог WPF DataGridColumnsBehavior —
    /// генерация колонок вынесена из code-behind, ViewModel отдает чистые дескрипторы.
    /// </summary>
    /// <remarks>
    /// TODO: Тех. долг / этап D. Не перенесено: подсветка локального переопределения значения
    /// атрибута (Moccasin, ValueOverrideStates/ValueOverrideToolTips) и редактируемый ComboBox
    /// с вводом формулы (в Avalonia ComboBox не editable).
    /// </remarks>
    public class DataGridColumnsBehavior
    {
        private DataGridColumnsBehavior()
        {
        }

        private static readonly StateToColorConverter StateToColor = new();
        private static readonly EnumDisplayAttributeConverter EnumDisplay = new();

        /// <summary>Источник дескрипторов колонок для динамического DataGrid.</summary>
        public static readonly AttachedProperty<IEnumerable?> ColumnsSourceProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumnsBehavior, DataGrid, IEnumerable?>("ColumnsSource");

        static DataGridColumnsBehavior()
        {
            ColumnsSourceProperty.Changed.AddClassHandler<DataGrid>(
                (grid, e) => RebuildColumns(grid, e.NewValue as IEnumerable));
        }

        public static IEnumerable? GetColumnsSource(DataGrid element) => element.GetValue(ColumnsSourceProperty);

        public static void SetColumnsSource(DataGrid element, IEnumerable? value) => element.SetValue(ColumnsSourceProperty, value);

        private static void RebuildColumns(DataGrid dataGrid, IEnumerable? columnsSource)
        {
            dataGrid.Columns.Clear();

            if (columnsSource == null)
            {
                return;
            }

            foreach (var column in columnsSource.OfType<ChildCollectionTableColumn>().OrderBy(x => x.Order))
            {
                dataGrid.Columns.Add(CreateColumn(column));
            }
        }

        private static DataGridColumn CreateColumn(ChildCollectionTableColumn column)
        {
            if (column.Key == nameof(IMainEntityModel.State))
            {
                return CreateStateColumn(column);
            }

            return column.HasValueOptions
                ? CreateComboBoxColumn(column)
                : CreateTextColumn(column);
        }

        /// <summary>Узкая цветовая колонка состояния (как в TreeView и таблице атрибутов).</summary>
        private static DataGridColumn CreateStateColumn(ChildCollectionTableColumn column)
        {
            var header = new TextBlock { Text = string.Empty, MinWidth = 7 };
            ToolTip.SetTip(header, column.HeaderToolTip ?? column.Header);

            var column1 = column;
            var col = new DataGridTemplateColumn
            {
                Header = header,
                IsReadOnly = true,
                Width = new DataGridLength(7),
                MinWidth = 7,
                CellTemplate = new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
                {
                    var border = new Border();
                    border.Bind(Border.BackgroundProperty, new Binding($"[{column1.BindingKey}]") { Converter = StateToColor });
                    border.Bind(ToolTip.TipProperty, new Binding($"[{column1.BindingKey}]")
                    {
                        Converter = EnumDisplay,
                        ConverterParameter = "Description",
                    });
                    return border;
                }),
            };
            col.CellStyleClasses.Add("statusStripe");
            return col;
        }

        /// <summary>Колонка выбора значения (combo) для редактируемых одиночных атрибутов.</summary>
        private static DataGridColumn CreateComboBoxColumn(ChildCollectionTableColumn column)
        {
            var column1 = column;
            var template = new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
            {
                var comboBox = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
                comboBox.Bind(ItemsControl.ItemsSourceProperty, new Binding($"ValueOptions[{column1.BindingKey}]"));
                comboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding($"[{column1.BindingKey}]") { Mode = BindingMode.TwoWay });
                comboBox.ItemTemplate = new FuncDataTemplate<object>((_, _) =>
                {
                    var textBlock = new TextBlock();
                    textBlock.Bind(TextBlock.TextProperty, new Binding("Name"));
                    return textBlock;
                });
                return comboBox;
            });

            return new DataGridTemplateColumn
            {
                Header = CreateHeader(column),
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = column.IsAttribute ? 120 : 80,
                CellTemplate = template,
                CellEditingTemplate = template,
            };
        }

        /// <summary>Текстовая колонка для readonly и простых редактируемых значений.</summary>
        private static DataGridColumn CreateTextColumn(ChildCollectionTableColumn column)
        {
            var binding = new Binding($"[{column.BindingKey}]")
            {
                Mode = column.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            };

            if (column.Key.EndsWith("At", StringComparison.OrdinalIgnoreCase))
            {
                binding.StringFormat = "{0:yyyy-MM-dd HH:mm}";
            }

            return new DataGridTextColumn
            {
                Header = CreateHeader(column),
                Binding = binding,
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = column.IsAttribute ? 120 : 80,
            };
        }

        private static object CreateHeader(ChildCollectionTableColumn column)
        {
            if (string.IsNullOrWhiteSpace(column.HeaderToolTip))
            {
                return column.Header;
            }

            var textBlock = new TextBlock { Text = column.Header };
            ToolTip.SetTip(textBlock, column.HeaderToolTip);
            return textBlock;
        }
    }
}
