using Microsoft.Extensions.DependencyInjection;
using Philadelphus.WpfApplication.Factories.Interfaces;
using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Factories.Implementations
{
    internal class ExtensionsControlVMFactory : IExtensionsControlVMFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public ExtensionsControlVMFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public ExtensionsControlVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            return ActivatorUtilities.CreateInstance<ExtensionsControlVM>(_serviceProvider, repositoryExplorerControlVM);
        }
    }
}
