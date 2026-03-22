using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    public interface IInfrastructureRepositoryFactory
    {
        public IInfrastructureRepository Create(InfrastructureTypes infrastructureType, InfrastructureEntityGroups entityGroup, string connectionString);
    }
}
