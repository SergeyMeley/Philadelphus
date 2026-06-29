using global::Avalonia.Controls;
using global::Avalonia.Interactivity;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Окно «О программе». DataContext (AboutWindowVM) задаётся через IWindowService.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
    }
}
