using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Окно деталей сущности. DataContext (DetailsWindowVM) задаётся снаружи через IWindowService.
    /// </summary>
    public partial class DetailsWindow : Window
    {
        public DetailsWindow()
        {
            InitializeComponent();
        }
    }
}
