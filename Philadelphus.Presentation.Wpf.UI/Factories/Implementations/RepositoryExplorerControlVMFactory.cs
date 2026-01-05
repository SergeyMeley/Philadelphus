using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
{
    internal class RepositoryExplorerControlVMFactory : IRepositoryExplorerControlVMFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public RepositoryExplorerControlVMFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public RepositoryExplorerControlVM Create(TreeRepositoryVM repositoryVM)
        {
            return ActivatorUtilities.CreateInstance<RepositoryExplorerControlVM>(_serviceProvider, repositoryVM);
        }
    }
}
