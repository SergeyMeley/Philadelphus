using Microsoft.Extensions.DependencyInjection;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using Philadelphus.WpfApplication.Views.Controls;
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
                    var context = _serviceProvider.GetRequiredService<MainWindowVM>();
                    var appVM = _serviceProvider.GetRequiredService<ApplicationVM>();
                    context.RepositoryExplorerVM = appVM.RepositoryCollectionVM.CurrentRepositoryExplorerVM;
                    mainWindow.DataContext = context;
                    mainWindow.Show();
                    var launchWindow = _serviceProvider.GetRequiredService<LaunchWindow>();
                    launchWindow.Hide();
                });
            }
        }
        public RelayCommand OpenLaunchWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var launchWindow = _serviceProvider.GetRequiredService<LaunchWindow>();
                    launchWindow.Show();
                });
            }
        }
        public RelayCommand OpenRepositoryMemberDetailsWindowCommand
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
