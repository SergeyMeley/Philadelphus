using Philadelphus.WpfApplication.ViewModels.SupportiveViewModels;
using Philadelphus.WpfApplication.Views;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationCommandsVM
    {
        private ApplicationVM _applicationVM;
        public ApplicationVM ApplicationVM { get { return _applicationVM; } }

        private ApplicationWindowsVM _applicationWindowsVM;
        public ApplicationWindowsVM ApplicationWindowsVM { get => _applicationWindowsVM; }
        public ApplicationCommandsVM(ApplicationVM applicationVM, ApplicationWindowsVM applicationWindowsVM)
        {
            _applicationVM = applicationVM;
            _applicationWindowsVM = applicationWindowsVM;
        }
        public RelayCommand OpenMainWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_applicationWindowsVM.MainWindow == null)
                        _applicationWindowsVM.MainWindow = new MainWindow() { DataContext = _applicationVM };
                    _applicationWindowsVM.MainWindow.Show();
                });
            }
        }
        public RelayCommand OpenDataStoragesSettingsWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_applicationWindowsVM.DataStoragesSettingsWindow == null)
                        _applicationWindowsVM.DataStoragesSettingsWindow = new DataStoragesSettingsWindow() { DataContext = _applicationVM.DataStoragesSettingsVM };
                    _applicationWindowsVM.DataStoragesSettingsWindow.Show();
                });
            }
        }

        public RelayCommand OpenRepositoryCreationWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var window = new RepositoryCreationWindow() { DataContext = this };
                    window.Show();
                    if (_applicationWindowsVM.RepositoryCreationWindow == null)
                        _applicationWindowsVM.RepositoryCreationWindow = new RepositoryCreationWindow() { DataContext = _applicationVM.RepositoryCreationVM };
                    _applicationWindowsVM.DataStoragesSettingsWindow.Show();
                });
            }
        }
    }
}
