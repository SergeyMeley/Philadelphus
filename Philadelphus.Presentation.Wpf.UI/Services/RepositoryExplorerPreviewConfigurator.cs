using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    internal static class RepositoryExplorerPreviewConfigurator
    {
        internal static void ConfigureAsReadonly(FrameworkElement previewRoot)
        {
            foreach (var button in FindVisualChildren<Button>(previewRoot))
            {
                button.Visibility = Visibility.Collapsed;
            }

            foreach (var textBox in FindVisualChildren<TextBox>(previewRoot))
            {
                textBox.IsReadOnly = true;
            }

            foreach (var comboBox in FindVisualChildren<ComboBox>(previewRoot))
            {
                comboBox.IsEnabled = false;
            }

            foreach (var dataGrid in FindVisualChildren<DataGrid>(previewRoot))
            {
                dataGrid.IsReadOnly = true;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
                yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (var nestedChild in FindVisualChildren<T>(child))
                {
                    yield return nestedChild;
                }
            }
        }
    }
}
