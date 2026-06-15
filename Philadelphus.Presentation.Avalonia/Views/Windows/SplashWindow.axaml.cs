using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Простое окно запуска с анимацией (название приложения + индикатор), показывается, пока
    /// поднимается Host. Не повторяет WPF SplashWindow — это лёгкая визуальная заглушка.
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
        }
    }
}
