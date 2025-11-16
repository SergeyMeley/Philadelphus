using Philadelphus.WpfApplication.Views.Windows;

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
                    var currentRepositoryVM = ApplicationVM.LaunchVM.RepositoryCollectionVM.CurrentRepositoryExplorerVM;
                    if (currentRepositoryVM != null)
                    {
                        var headerVM = ApplicationVM.LaunchVM.RepositoryHeadersCollectionVM.TreeRepositoryHeadersVMs.FirstOrDefault(x => x.Guid == currentRepositoryVM.Guid);
                        if (headerVM == null)
                        {
                            headerVM = ApplicationVM.LaunchVM.RepositoryHeadersCollectionVM.AddTreeRepositoryHeaderVMFromTreeRepositoryVM(currentRepositoryVM);
                            
                        }
                        headerVM.LastOpening = DateTime.UtcNow;
                    }
                    
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
