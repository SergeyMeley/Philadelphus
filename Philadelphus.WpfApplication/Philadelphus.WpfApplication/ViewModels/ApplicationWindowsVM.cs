using Philadelphus.WpfApplication.Views;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationWindowsVM   //TODO: Удалить и брать окна из DI
    {
        private LaunchWindow _launchWindow;
        public LaunchWindow LaunchWindow { get => _launchWindow; set => _launchWindow = value; }

        private MainWindow _mainWindow;
        public MainWindow MainWindow { get => _mainWindow; set => _mainWindow = value; }
    }
}
