using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Возвращает фокус в текущую ячейку DataGrid по запросу из ViewModel.
    /// </summary>
    public static class DataGridCellFocusRequestBehavior
    {
        public static readonly DependencyProperty FocusRequestIdProperty =
            DependencyProperty.RegisterAttached(
                "FocusRequestId",
                typeof(int),
                typeof(DataGridCellFocusRequestBehavior),
                new PropertyMetadata(0, OnFocusRequestIdChanged));

        public static int GetFocusRequestId(DependencyObject obj)
        {
            return (int)obj.GetValue(FocusRequestIdProperty);
        }

        public static void SetFocusRequestId(DependencyObject obj, int value)
        {
            obj.SetValue(FocusRequestIdProperty, value);
        }

        public static readonly DependencyProperty TargetColumnHeaderProperty =
            DependencyProperty.RegisterAttached(
                "TargetColumnHeader",
                typeof(string),
                typeof(DataGridCellFocusRequestBehavior),
                new PropertyMetadata(string.Empty));

        public static string GetTargetColumnHeader(DependencyObject obj)
        {
            return (string)obj.GetValue(TargetColumnHeaderProperty);
        }

        public static void SetTargetColumnHeader(DependencyObject obj, string value)
        {
            obj.SetValue(TargetColumnHeaderProperty, value);
        }

        private static void OnFocusRequestIdChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not DataGrid dataGrid
                || Equals(e.OldValue, e.NewValue))
            {
                return;
            }

            dataGrid.Dispatcher.BeginInvoke(
                () => FocusRequestedCell(dataGrid),
                DispatcherPriority.Input);
        }

        private static void FocusRequestedCell(DataGrid dataGrid)
        {
            if (dataGrid.IsEnabled == false)
            {
                return;
            }

            var item = dataGrid.CurrentItem
                ?? dataGrid.SelectedItem
                ?? dataGrid.Items.Cast<object>().FirstOrDefault();
            var column = ResolveTargetColumn(dataGrid);

            if (item == null || column == null)
            {
                return;
            }

            dataGrid.Focus();
            dataGrid.ScrollIntoView(item, column);
            dataGrid.CurrentCell = new DataGridCellInfo(item, column);
            if (dataGrid.SelectionUnit == DataGridSelectionUnit.FullRow)
            {
                dataGrid.SelectedItem = item;
            }
            else
            {
                dataGrid.SelectedCells.Clear();
                dataGrid.SelectedCells.Add(dataGrid.CurrentCell);
            }

            Keyboard.Focus(dataGrid);
        }

        private static DataGridColumn? ResolveTargetColumn(DataGrid dataGrid)
        {
            var targetHeader = GetTargetColumnHeader(dataGrid);
            if (string.IsNullOrWhiteSpace(targetHeader) == false)
            {
                var column = dataGrid.Columns.FirstOrDefault(x =>
                    string.Equals(x.Header?.ToString(), targetHeader, StringComparison.Ordinal));
                if (column != null)
                {
                    return column;
                }
            }

            return dataGrid.CurrentCell.Column
                ?? dataGrid.Columns.FirstOrDefault();
        }
    }
}
