using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    internal interface IMainWindowVMFactory
    {
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
