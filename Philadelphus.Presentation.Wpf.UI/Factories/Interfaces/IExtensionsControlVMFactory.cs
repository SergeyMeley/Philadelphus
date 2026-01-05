using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    public interface IExtensionsControlVMFactory
    {
        public ExtensionsControlVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
