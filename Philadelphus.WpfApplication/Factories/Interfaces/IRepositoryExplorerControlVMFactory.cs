using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Factories.Interfaces
{
    internal interface IRepositoryExplorerControlVMFactory
    {
        public RepositoryExplorerControlVM Create(TreeRepositoryVM repositoryVM);
    }
}
