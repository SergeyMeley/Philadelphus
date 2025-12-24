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
    internal class MainWindowVMFactory : IMainWindowVMFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public MainWindowVMFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            return ActivatorUtilities.CreateInstance<MainWindowVM>(_serviceProvider, repositoryExplorerControlVM);
        }
    }
}
