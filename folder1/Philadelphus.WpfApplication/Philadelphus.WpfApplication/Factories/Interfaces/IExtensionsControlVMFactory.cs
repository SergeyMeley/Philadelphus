using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Factories.Interfaces
{
    public interface IExtensionsControlVMFactory
    {
        public ExtensionsControlVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
