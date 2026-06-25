using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Окно редактора формул. DataContext (FormulaTestControlVM) задаётся снаружи через IWindowService.
    /// </summary>
    public partial class FormulaEditorWindow : Window
    {
        public FormulaEditorWindow()
        {
            InitializeComponent();
        }
    }
}
