using Philadelphus.Presentation.Wpf.UI.Views.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    public class ApplicationWindowsVM   //TODO: Удалить и брать окна из DI
    {
        private LaunchWindow _launchWindow;
        public LaunchWindow LaunchWindow { get => _launchWindow; set => _launchWindow = value; }

        private MainWindow _mainWindow;
        public MainWindow MainWindow { get => _mainWindow; set => _mainWindow = value; }
    }
}
