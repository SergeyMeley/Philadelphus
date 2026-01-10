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
            _serviceProvider = serviceProvider;
        }
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            if (repositoryExplorerControlVM == null)
                throw new ArgumentNullException();
            return ActivatorUtilities.CreateInstance<MainWindowVM>(_serviceProvider, repositoryExplorerControlVM);
        }
    }
}
