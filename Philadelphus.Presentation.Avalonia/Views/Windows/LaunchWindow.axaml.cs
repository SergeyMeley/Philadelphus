using System.Linq;

using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для LaunchWindow.axaml
    /// </summary>
    public partial class LaunchWindow : Window
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LaunchWindow" />.
        /// DataContext (LaunchWindowVM) задаётся снаружи через IWindowService.
        /// </summary>
        public LaunchWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // Пока открыто хотя бы одно MainWindow — не закрываемся, а прячемся (как в WPF).
            if (Lifetime?.Windows.OfType<MainWindow>().Any() == true)
            {
                Hide();
                e.Cancel = true;
            }

            base.OnClosing(e);
        }

        private static IClassicDesktopStyleApplicationLifetime? Lifetime
            => global::Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    }
}
