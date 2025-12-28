using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    public interface IExtensionsControlVMFactory
    {
        public ExtensionsControlVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
