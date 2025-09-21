using Philadelphus.WpfApplication.Views;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationWindowsVM
    {
        private LaunchWindow _launchWindow;
        public LaunchWindow LaunchWindow { get => _launchWindow; set => _launchWindow = value; }

        private MainWindow _mainWindow;
        public MainWindow MainWindow { get => _mainWindow; set => _mainWindow = value; }

        private DataStoragesSettingsWindow _dataStoragesSettingsWindow;
        public DataStoragesSettingsWindow DataStoragesSettingsWindow { get => _dataStoragesSettingsWindow; set => _dataStoragesSettingsWindow = value; }

        private RepositoryCreationWindow _repositoryCreationWindow;
        public RepositoryCreationWindow RepositoryCreationWindow { get => _repositoryCreationWindow; set => _repositoryCreationWindow = value; }
        
    }
}
