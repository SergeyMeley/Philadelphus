using Microsoft.Extensions.DependencyInjection;
using Philadelphus.WpfApplication.Views.Windows;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationCommandsVM
    {
        private readonly IServiceProvider _serviceProvider;
        public ApplicationCommandsVM(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public RelayCommand OpenMainWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var launchVM = _serviceProvider.GetRequiredService<LaunchVM>();
                    var currentRepositoryVM = launchVM.RepositoryCollectionVM.CurrentRepositoryExplorerVM;
                    if (currentRepositoryVM != null)
                    {
                        var headerVM = launchVM.RepositoryHeadersCollectionVM.TreeRepositoryHeadersVMs.FirstOrDefault(x => x.Guid == currentRepositoryVM.Guid);
                        if (headerVM == null)
                        {
                            headerVM = launchVM.RepositoryHeadersCollectionVM.AddTreeRepositoryHeaderVMFromTreeRepositoryVM(currentRepositoryVM);

                        }
                        headerVM.LastOpening = DateTime.UtcNow;
                    }

                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                });
            }
        }
        public RelayCommand OpenRepositoryMemberDetailsWindow
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var launchVM = _serviceProvider.GetRequiredService<LaunchVM>();
                    var currentRepositoryMemberVM = launchVM.RepositoryCollectionVM.CurrentRepositoryExplorerVM.SelectedRepositoryMember;
                    var window = new DetailsWindow(currentRepositoryMemberVM);
                    window.Show();
                });
            }
        }
    }
}
