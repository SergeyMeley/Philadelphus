using System.Linq;

using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Главное окно приложения. DataContext (MainWindowVM) задаётся снаружи через IWindowService.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // Нет других MainWindow — завершаем приложение целиком (как в WPF).
            var otherMainWindows = Lifetime?.Windows.OfType<MainWindow>().Count(w => !ReferenceEquals(w, this)) ?? 0;
            if (otherMainWindows == 0)
            {
                Lifetime?.Shutdown();
            }

            base.OnClosing(e);
        }

        private static IClassicDesktopStyleApplicationLifetime? Lifetime
            => global::Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    }
}
