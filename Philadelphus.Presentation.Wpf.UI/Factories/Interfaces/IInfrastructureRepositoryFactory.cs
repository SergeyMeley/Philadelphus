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
    /// <summary>
    /// Задает контракт для работы с инфраструктурного репозитория.
    /// </summary>
    public interface IInfrastructureRepositoryFactory
    {
        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="infrastructureType">Тип инфраструктуры.</param>
        /// <param name="entityGroup">Группа инфраструктурных сущностей.</param>
        /// <param name="connectionString">Строка подключения.</param>
        /// <returns>Созданный объект.</returns>
        public IInfrastructureRepository Create(InfrastructureTypes infrastructureType, InfrastructureEntityGroups entityGroup, string connectionString);
    }
}
