using Microsoft.Extensions.DependencyInjection;
using Philadelphus.WpfApplication.Factories.Interfaces;
using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Factories.Implementations
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
