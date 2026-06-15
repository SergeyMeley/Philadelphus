using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Окно списка значений атрибута. DataContext (RepositoryExplorerControlVM) задаётся снаружи через IWindowService.
    /// </summary>
    public partial class AttributeValuesCollectionWindow : Window
    {
        public AttributeValuesCollectionWindow()
        {
            InitializeComponent();
        }
    }
}
