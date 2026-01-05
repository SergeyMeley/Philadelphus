using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    internal interface IRepositoryExplorerControlVMFactory
    {
        public RepositoryExplorerControlVM Create(TreeRepositoryVM repositoryVM);
    }
}
