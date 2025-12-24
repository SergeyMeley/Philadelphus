using Microsoft.Extensions.DependencyInjection;
using Philadelphus.WpfApplication.Factories.Implementations;
using Philadelphus.WpfApplication.Factories.Interfaces;
using Philadelphus.WpfApplication.Infrastructure;
using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
using Philadelphus.WpfApplication.Views.Controls;
using Philadelphus.WpfApplication.Views.Windows;
using System;

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
                    var launchVM = _serviceProvider.GetRequiredService<LaunchWindowVM>();
                    var currentRepositoryVM = launchVM.RepositoryCollectionVM.CurrentRepositoryVM;
                    if (currentRepositoryVM != null)
                    {
                        var headerVM = launchVM.RepositoryHeadersCollectionVM.TreeRepositoryHeadersVMs.FirstOrDefault(x => x.Guid == currentRepositoryVM.Guid);
                        if (headerVM == null)
                        {
                            headerVM = launchVM.RepositoryHeadersCollectionVM.AddTreeRepositoryHeaderVMFromTreeRepositoryVM(currentRepositoryVM);

                        }
                        headerVM.LastOpening = DateTime.UtcNow;
                    }

                    var appVM = _serviceProvider.GetRequiredService<ApplicationVM>();
                    var repositoryExplorerControlVM = _serviceProvider.GetRequiredService<IRepositoryExplorerControlVMFactory>().Create(currentRepositoryVM);
                    var mainWindowVM = _serviceProvider.GetRequiredService<IMainWindowVMFactory>().Create(repositoryExplorerControlVM);
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    //context.RepositoryExplorerVM = _serviceProvider.GetRequiredService<RepositoryExplorerControlVM>();
                    //context.RepositoryExplorerVM.TreeRepositoryVM = appVM.RepositoryCollectionVM.CurrentRepositoryVM;
                    mainWindow.DataContext = mainWindowVM;
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

        public RelayCommand OpenDataStoragesSettingsControlCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var qwe = obj.GetType();
                    var launchWindow = _serviceProvider.GetRequiredService<LaunchWindow>();
                    launchWindow.Show();
                });
            }
        }
    }
}
