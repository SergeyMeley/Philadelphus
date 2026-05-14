using Philadelphus.Presentation.Wpf.UI.Views.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    /// <summary>
    /// Модель представления для окон приложения.
    /// </summary>
    public class ApplicationWindowsVM   //TODO: Удалить и брать окна из DI
    {
        private LaunchWindow _launchWindow;
        
        /// <summary>
        /// Стартовое окно.
        /// </summary>
        public LaunchWindow LaunchWindow { get => _launchWindow; set => _launchWindow = value; }

        private MainWindow _mainWindow;
       
        /// <summary>
        /// Главное окно.
        /// </summary>
        public MainWindow MainWindow { get => _mainWindow; set => _mainWindow = value; }
    }
}
