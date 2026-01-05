using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
{
    internal class ExtensionsControlVMFactory : IExtensionsControlVMFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public ExtensionsControlVMFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public ExtensionsControlVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            return ActivatorUtilities.CreateInstance<ExtensionsControlVM>(_serviceProvider, repositoryExplorerControlVM);
        }
    }
}
