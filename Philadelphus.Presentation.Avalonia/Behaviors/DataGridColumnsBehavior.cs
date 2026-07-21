using System;
using System.Collections;
using System.Linq;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Data;
using global::Avalonia.Input;
using global::Avalonia.Layout;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.Avalonia.Controls;
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
    /// Подсветка локального переопределения значения атрибута (Moccasin,
    /// ValueOverrideStates/ValueOverrideToolTips) перенесена — см. WrapWithOverride.
    /// Ячейка значения атрибута работает идентично таблице атрибутов (см. тех-долг I, п.2):
    /// просмотр — DisplayTexts (результат формулы/код ошибки/значение), редактирование —
    /// editable ComboBox с вводом формулы (Text=EditText), общая логика — AttributeValueText.
    /// </remarks>
    public sealed class DataGridColumnsBehavior
    {
        private DataGridColumnsBehavior()
        {
        }

        private static readonly StateToColorConverter StateToColor = new();
        private static readonly EnumDisplayAttributeConverter EnumDisplay = new();
        private static readonly BoolToBrushConverter BoolToBrush = new();

        // Подсветка «переопределено» (Moccasin, статусный цвет) + чёрный текст для читаемости в Dark,
        // как в таблице атрибутов (см. Attributes.axaml). Пустая часть параметра → UnsetValue (дефолт темы).
        private const string OverrideBackgroundParameter = "Moccasin|";
        private const string OverrideForegroundParameter = "Black|";

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

            return column.ColumnType switch
            {
                ChildCollectionTableColumnType.ComboBox => CreateComboBoxColumn(column),
                ChildCollectionTableColumnType.CheckBox => CreateCheckBoxColumn(column),
                _ => CreateTextColumn(column),
            };
        }

        /// <summary>
        /// Интерактивная boolean-колонка с доступностью и подсказкой отдельной ячейки.
        /// </summary>
        private static DataGridColumn CreateCheckBoxColumn(ChildCollectionTableColumn column)
        {
            var template = new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
            {
                var checkBox = new CheckBox
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                checkBox.Bind(ToggleButton.IsCheckedProperty, new Binding($"[{column.BindingKey}]")
                {
                    Mode = BindingMode.TwoWay,
                });
                checkBox.Bind(InputElement.IsEnabledProperty, new Binding($"CellEnabledStates[{column.BindingKey}]"));
                checkBox.Bind(ToolTip.TipProperty, new Binding($"CellToolTips[{column.BindingKey}]"));
                return checkBox;
            });

            return new DataGridTemplateColumn
            {
                Header = CreateHeader(column),
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = 60,
                CellTemplate = template,
            };
        }

        /// <summary>Узкая цветовая колонка состояния (как в TreeView и таблице атрибутов).</summary>
        private static DataGridColumn CreateStateColumn(ChildCollectionTableColumn column)
        {
            var header = new TextBlock { Text = string.Empty, MinWidth = 7 };
            ToolTip.SetTip(header, column.HeaderToolTip ?? column.Header);

            var col = new DataGridTemplateColumn
            {
                Header = header,
                IsReadOnly = true,
                Width = new DataGridLength(7),
                MinWidth = 7,
                CellTemplate = new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
                {
                    var stripe = new StateVisibilityStripe();
                    stripe.Bind(StateVisibilityStripe.ParentOwnerStateProperty, new Binding(nameof(ChildCollectionTableRow.ParentOwnerAggregateState)));
                    stripe.Bind(StateVisibilityStripe.StateProperty, new Binding(nameof(ChildCollectionTableRow.State)));
                    stripe.Bind(StateVisibilityStripe.ChildContentStateProperty, new Binding(nameof(ChildCollectionTableRow.ChildContentAggregateState)));
                    stripe.Bind(StateVisibilityStripe.StateVisibilityToolTipProperty, new Binding(nameof(ChildCollectionTableRow.StateVisibilityToolTip)));
                    return stripe;
                }),
            };
            col.CellStyleClasses.Add("statusStripe");
            return col;
        }

        /// <summary>Колонка выбора значения (combo) для редактируемых одиночных атрибутов.</summary>
        private static DataGridColumn CreateComboBoxColumn(ChildCollectionTableColumn column)
        {
            var column1 = column;

            FuncDataTemplate<ChildCollectionTableRow> CreateComboTemplate() => new((_, _) =>
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

            // Неатрибутные combo-колонки: combo всегда (как было).
            if (column.IsAttribute == false)
            {
                var template = CreateComboTemplate();
                return new DataGridTemplateColumn
                {
                    Header = CreateHeader(column),
                    IsReadOnly = column.IsReadOnly,
                    Width = DataGridLength.Auto,
                    MinWidth = 80,
                    CellTemplate = template,
                    CellEditingTemplate = template,
                };
            }

            // Атрибутные: в покое — отображаемый текст значения (результат формулы / код ошибки /
            // значение) на подсветке, combo появляется только при редактировании. ПОЛНОСТЬЮ как
            // ячейка значения в таблице атрибутов (Attributes.axaml): просмотр — DisplayTexts,
            // редактирование — editable ComboBox с вводом формулы (Text=EditText).
            var cellTemplate = new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
            {
                var text = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 0),
                };
                text.Bind(TextBlock.TextProperty, new Binding($"DisplayTexts[{column1.BindingKey}]"));
                text.Bind(TextBlock.ForegroundProperty, OverrideForegroundBinding(column1.BindingKey));
                return WrapWithOverride(column1, text);
            });

            var result = new DataGridTemplateColumn
            {
                Header = CreateHeader(column),
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = 120,
                CellTemplate = cellTemplate,
            };

            if (column.IsReadOnly == false)
            {
                result.CellEditingTemplate = CreateEditableFormulaComboTemplate(column1);
            }

            return result;
        }

        /// <summary>
        /// Редактируемый ComboBox ввода значения атрибута — идентичен ячейке таблицы атрибутов
        /// (Avalonia 11.3: IsEditable + Text). SelectedItem НЕ привязываем (сбрасывает значение при
        /// ручном вводе); всё идёт через Text=EditText: выбор из списка пишет «[uuid]»
        /// (TextSearch.TextBinding), сеттер EditText присваивает значение по ссылке, ввод формулы/
        /// значения — тоже через Text.
        /// </summary>
        private static FuncDataTemplate<ChildCollectionTableRow> CreateEditableFormulaComboTemplate(ChildCollectionTableColumn column)
        {
            return new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
            {
                var comboBox = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    IsEditable = true,
                    IsTextSearchEnabled = false,
                };
                comboBox.Bind(ItemsControl.ItemsSourceProperty, new Binding($"ValueOptions[{column.BindingKey}]"));
                comboBox.Bind(ComboBox.TextProperty, new Binding($"EditText[{column.BindingKey}]")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
                });
                comboBox.SetValue(TextSearch.TextBindingProperty, new Binding("Uuid") { StringFormat = "[{0}]" });
                comboBox.ItemTemplate = new FuncDataTemplate<object>((_, _) =>
                {
                    var textBlock = new TextBlock();
                    textBlock.Bind(TextBlock.TextProperty, new Binding("Name"));
                    return textBlock;
                });
                return comboBox;
            });
        }

        /// <summary>Текстовая колонка для readonly и простых редактируемых значений.</summary>
        private static DataGridColumn CreateTextColumn(ChildCollectionTableColumn column)
        {
            // Атрибутные колонки получают подсветку «переопределено» + тултип (как в таблице атрибутов),
            // поэтому строятся шаблоном, а не DataGridTextColumn.
            if (column.IsAttribute)
            {
                return CreateAttributeTextColumn(column);
            }

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
                MinWidth = 80,
            };
        }

        /// <summary>
        /// Текстовая колонка атрибута: подсветка «переопределено» (Moccasin) + тултип на ячейке,
        /// чёрный текст при переопределении. Редактирование — TextBox в CellEditingTemplate.
        /// </summary>
        private static DataGridColumn CreateAttributeTextColumn(ChildCollectionTableColumn column)
        {
            var column1 = column;

            var cellTemplate = new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
            {
                var text = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 0),
                };
                text.Bind(TextBlock.TextProperty, new Binding($"DisplayTexts[{column1.BindingKey}]"));
                text.Bind(TextBlock.ForegroundProperty, OverrideForegroundBinding(column1.BindingKey));
                return WrapWithOverride(column1, text);
            });

            var result = new DataGridTemplateColumn
            {
                Header = CreateHeader(column),
                IsReadOnly = column.IsReadOnly,
                Width = DataGridLength.Auto,
                MinWidth = 120,
                CellTemplate = cellTemplate,
            };

            if (column.IsReadOnly == false)
            {
                result.CellEditingTemplate = new FuncDataTemplate<ChildCollectionTableRow>((_, _) =>
                {
                    var box = new TextBox { VerticalAlignment = VerticalAlignment.Stretch };
                    box.Bind(TextBox.TextProperty, new Binding($"[{column1.BindingKey}]") { Mode = BindingMode.TwoWay });
                    return box;
                });
            }

            return result;
        }

        /// <summary>Оборачивает контент ячейки атрибута в Border с подсветкой переопределения и тултипом.</summary>
        private static Border WrapWithOverride(ChildCollectionTableColumn column, Control content)
        {
            var border = new Border { Child = content };
            border.Bind(Border.BackgroundProperty, new Binding($"ValueOverrideStates[{column.BindingKey}]")
            {
                Converter = BoolToBrush,
                ConverterParameter = OverrideBackgroundParameter,
            });
            border.Bind(ToolTip.TipProperty, new Binding($"ValueOverrideToolTips[{column.BindingKey}]"));
            return border;
        }

        /// <summary>Привязка цвета текста: чёрный при переопределении, иначе цвет темы.</summary>
        private static Binding OverrideForegroundBinding(string bindingKey)
            => new($"ValueOverrideStates[{bindingKey}]")
            {
                Converter = BoolToBrush,
                ConverterParameter = OverrideForegroundParameter,
            };

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
