using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Подсвечивает ячейки, выбранные пользователем как ссылки при редактировании формулы.
    /// </summary>
    public static class DataGridFormulaReferenceHighlightBehavior
    {
        private static readonly Brush[] ReferenceBrushes =
        [
            Brushes.DodgerBlue,
            Brushes.ForestGreen,
            Brushes.DarkOrange,
            Brushes.MediumVioletRed,
            Brushes.DarkViolet,
            Brushes.Teal,
        ];

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached(
                "IsActive",
                typeof(bool),
                typeof(DataGridFormulaReferenceHighlightBehavior),
                new PropertyMetadata(false, OnIsActiveChanged));

        public static bool GetIsActive(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsActiveProperty);
        }

        public static void SetIsActive(DependencyObject obj, bool value)
        {
            obj.SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty AttributeColumnsOnlyProperty =
            DependencyProperty.RegisterAttached(
                "AttributeColumnsOnly",
                typeof(bool),
                typeof(DataGridFormulaReferenceHighlightBehavior),
                new PropertyMetadata(false));

        public static bool GetAttributeColumnsOnly(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttributeColumnsOnlyProperty);
        }

        public static void SetAttributeColumnsOnly(DependencyObject obj, bool value)
        {
            obj.SetValue(AttributeColumnsOnlyProperty, value);
        }

        public static readonly DependencyProperty TargetColumnHeaderProperty =
            DependencyProperty.RegisterAttached(
                "TargetColumnHeader",
                typeof(string),
                typeof(DataGridFormulaReferenceHighlightBehavior),
                new PropertyMetadata(string.Empty));

        public static string GetTargetColumnHeader(DependencyObject obj)
        {
            return (string)obj.GetValue(TargetColumnHeaderProperty);
        }

        public static void SetTargetColumnHeader(DependencyObject obj, string value)
        {
            obj.SetValue(TargetColumnHeaderProperty, value);
        }

        private static readonly DependencyProperty HighlightedCellsProperty =
            DependencyProperty.RegisterAttached(
                "HighlightedCells",
                typeof(ObservableCollection<DataGridCell>),
                typeof(DataGridFormulaReferenceHighlightBehavior),
                new PropertyMetadata(null));

        private static readonly DependencyProperty NextBrushIndexProperty =
            DependencyProperty.RegisterAttached(
                "NextBrushIndex",
                typeof(int),
                typeof(DataGridFormulaReferenceHighlightBehavior),
                new PropertyMetadata(0));

        private static void OnIsActiveChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not DataGrid dataGrid)
            {
                return;
            }

            if ((bool)e.OldValue == false && (bool)e.NewValue)
            {
                dataGrid.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                return;
            }

            if ((bool)e.OldValue && (bool)e.NewValue == false)
            {
                dataGrid.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                ClearHighlights(dataGrid);
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dataGrid
                || GetIsActive(dataGrid) == false
                || e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            var cell = FindVisualParent<DataGridCell>(source);
            if (cell == null || IsAllowedCell(dataGrid, cell) == false)
            {
                return;
            }

            HighlightCell(dataGrid, cell);
        }

        private static bool IsAllowedCell(DataGrid dataGrid, DataGridCell cell)
        {
            if (GetAttributeColumnsOnly(dataGrid)
                && DataGridColumnsBehavior.GetIsAttributeColumn(cell.Column) == false)
            {
                return false;
            }

            var targetHeader = GetTargetColumnHeader(dataGrid);
            if (string.IsNullOrWhiteSpace(targetHeader))
            {
                return true;
            }

            return string.Equals(
                cell.Column.Header?.ToString(),
                targetHeader,
                StringComparison.Ordinal);
        }

        private static void HighlightCell(DataGrid dataGrid, DataGridCell cell)
        {
            var highlightedCells = GetHighlightedCells(dataGrid);
            if (highlightedCells.Contains(cell) == false)
            {
                highlightedCells.Add(cell);
            }

            var index = (int)dataGrid.GetValue(NextBrushIndexProperty);
            dataGrid.SetValue(NextBrushIndexProperty, (index + 1) % ReferenceBrushes.Length);

            cell.BorderBrush = ReferenceBrushes[index % ReferenceBrushes.Length];
            cell.BorderThickness = new Thickness(2);
        }

        private static ObservableCollection<DataGridCell> GetHighlightedCells(DataGrid dataGrid)
        {
            if (dataGrid.GetValue(HighlightedCellsProperty) is ObservableCollection<DataGridCell> result)
            {
                return result;
            }

            result = new ObservableCollection<DataGridCell>();
            dataGrid.SetValue(HighlightedCellsProperty, result);
            return result;
        }

        private static void ClearHighlights(DataGrid dataGrid)
        {
            var highlightedCells = GetHighlightedCells(dataGrid);
            foreach (var cell in highlightedCells)
            {
                cell.ClearValue(Control.BorderBrushProperty);
                cell.ClearValue(Control.BorderThicknessProperty);
            }

            highlightedCells.Clear();
            dataGrid.SetValue(NextBrushIndexProperty, 0);
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
