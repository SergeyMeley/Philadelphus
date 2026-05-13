using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
{
    internal class MainWindowVMFactory : IMainWindowVMFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public MainWindowVMFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            ArgumentNullException.ThrowIfNull(repositoryExplorerControlVM);

            return ActivatorUtilities.CreateInstance<MainWindowVM>(_serviceProvider, repositoryExplorerControlVM);
        }
    }
}
