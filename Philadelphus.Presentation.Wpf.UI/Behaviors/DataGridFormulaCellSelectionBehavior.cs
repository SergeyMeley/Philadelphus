using Philadelphus.Presentation.Wpf.UI.Models.Tables;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Передает во ViewModel выбранную атрибутную ячейку динамической таблицы наследников.
    /// </summary>
    public static class DataGridFormulaCellSelectionBehavior
    {
        /// <summary>
        /// Команда, получающая сведения о выбранной атрибутной ячейке таблицы наследников.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(DataGridFormulaCellSelectionBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Возвращает команду обработки выбора атрибутной ячейки.
        /// </summary>
        public static ICommand? GetCommand(DependencyObject obj)
        {
            return (ICommand?)obj.GetValue(CommandProperty);
        }

        /// <summary>
        /// Устанавливает команду обработки выбора атрибутной ячейки.
        /// </summary>
        public static void SetCommand(DependencyObject obj, ICommand? value)
        {
            obj.SetValue(CommandProperty, value);
        }

        private static void OnCommandChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not DataGrid dataGrid)
            {
                return;
            }

            if (e.OldValue != null)
            {
                dataGrid.SelectedCellsChanged -= OnSelectedCellsChanged;
                dataGrid.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            }

            if (e.NewValue != null)
            {
                dataGrid.SelectedCellsChanged += OnSelectedCellsChanged;
                dataGrid.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            }
        }

        private static void OnSelectedCellsChanged(object? sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is not DataGrid dataGrid
                || dataGrid.CurrentCell.Column == null)
            {
                return;
            }

            ExecuteSelectionCommand(dataGrid, dataGrid.CurrentCell.Item, dataGrid.CurrentCell.Column);
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dataGrid
                || e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            var cell = FindVisualParent<DataGridCell>(source);
            if (cell == null)
            {
                return;
            }

            // DataGrid не всегда успевает обновить CurrentCell до SelectedCellsChanged,
            // поэтому при клике выставляем текущую ячейку вручную и сразу отправляем selection во VM.
            dataGrid.Focus();
            dataGrid.CurrentCell = new DataGridCellInfo(cell.DataContext, cell.Column);
            dataGrid.SelectedCells.Clear();
            dataGrid.SelectedCells.Add(dataGrid.CurrentCell);

            ExecuteSelectionCommand(dataGrid, cell.DataContext, cell.Column);
        }

        private static void ExecuteSelectionCommand(
            DataGrid dataGrid,
            object item,
            DataGridColumn column)
        {
            if (item is not ChildCollectionTableRow row)
            {
                return;
            }

            var command = GetCommand(dataGrid);
            if (command == null)
            {
                return;
            }

            var selection = new ChildFormulaCellSelection(
                row.SourceUuid,
                DataGridColumnsBehavior.GetColumnKey(column),
                dataGrid.Items.IndexOf(row) + 1,
                DataGridColumnsBehavior.GetIsAttributeColumn(column));

            if (command.CanExecute(selection))
            {
                command.Execute(selection);
            }
        }

        private static T? FindVisualParent<T>(DependencyObject source)
            where T : DependencyObject
        {
            var current = source;
            while (current != null)
            {
                if (current is T result)
                {
                    return result;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
