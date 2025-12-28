using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
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
