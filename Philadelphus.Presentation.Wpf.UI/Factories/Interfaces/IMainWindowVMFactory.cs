using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    internal interface IMainWindowVMFactory
    {
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
