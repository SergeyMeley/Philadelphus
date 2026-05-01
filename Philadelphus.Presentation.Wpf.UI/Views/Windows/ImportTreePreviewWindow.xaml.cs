using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    public partial class ImportTreePreviewWindow : Window
    {
        private RepositoryExplorerControlVM? _viewModel;

        public ImportTreePreviewWindow()
        {
            InitializeComponent();
        }

        public void Initialize(RepositoryExplorerControlVM viewModel, string summaryText)
        {
            _viewModel = viewModel;
            TxtSummary.Text = summaryText;
            RepositoryExplorerPreview.DataContext = viewModel;
            ConfigureAsReadonlyPreview();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfigureAsReadonlyPreview()
        {
            foreach (var button in FindVisualChildren<Button>(RepositoryExplorerPreview))
            {
                button.Visibility = Visibility.Collapsed;
            }

            foreach (var textBox in FindVisualChildren<TextBox>(RepositoryExplorerPreview))
            {
                textBox.IsReadOnly = true;
            }

            foreach (var comboBox in FindVisualChildren<ComboBox>(RepositoryExplorerPreview))
            {
                comboBox.IsEnabled = false;
            }

            foreach (var dataGrid in FindVisualChildren<DataGrid>(RepositoryExplorerPreview))
            {
                dataGrid.IsReadOnly = true;
            }
        }

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
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
