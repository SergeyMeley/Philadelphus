using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    internal interface IRepositoryExplorerControlVMFactory
    {
        public RepositoryExplorerControlVM Create(TreeRepositoryVM repositoryVM);
    }
}
