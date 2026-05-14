using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    /// <summary>
    /// Модель представления для команд приложения.
    /// </summary>
    public class ApplicationCommandsVM
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationCommandsVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ApplicationCommandsVM(
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

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
                        var headerVM = launchVM.RepositoryHeadersCollectionVM.PhiladelphusRepositoryHeadersVMs.FirstOrDefault(x => x.Uuid == currentRepositoryVM.Uuid);
                        if (headerVM == null)
                        {
                            headerVM = launchVM.RepositoryHeadersCollectionVM.AddPhiladelphusRepositoryHeaderVMFromPhiladelphusRepositoryVM(currentRepositoryVM);
                        }
                        headerVM.LastOpening = DateTime.UtcNow;
                    }

                    var appVM = _serviceProvider.GetRequiredService<ApplicationVM>();
                    var repositoryExplorerControlVM = _serviceProvider.GetRequiredService<IRepositoryExplorerControlVMFactory>().Create(currentRepositoryVM);
                    var mainWindowVM = _serviceProvider.GetRequiredService<IMainWindowVMFactory>().Create(repositoryExplorerControlVM);
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.DataContext = mainWindowVM;
                    mainWindow.Topmost = true;
                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.Topmost = false;
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
