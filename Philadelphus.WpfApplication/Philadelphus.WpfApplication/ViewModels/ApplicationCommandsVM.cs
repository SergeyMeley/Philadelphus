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
                        _applicationWindowsVM.MainWindow = new MainWindow(_applicationVM);
                    _applicationWindowsVM.MainWindow.Show();
                    _applicationWindowsVM.LaunchWindow.Close();
                    _applicationWindowsVM.LaunchWindow = null;
                });
            }
        }
        public RelayCommand OpenDataStoragesSettingsWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _applicationWindowsVM.DataStoragesSettingsWindow = new DataStoragesSettingsWindow(_applicationVM.DataStoragesSettingsVM);
                    _applicationWindowsVM.DataStoragesSettingsWindow.ShowDialog();
                });
            }
        }

        public RelayCommand OpenRepositoryCreationWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _applicationVM.RepositoryCreationVM.OpenWindow();
                });
            }
        }
    }
}
