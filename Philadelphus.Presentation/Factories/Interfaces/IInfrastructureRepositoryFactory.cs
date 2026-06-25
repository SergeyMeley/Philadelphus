using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Presentation.Factories.Interfaces
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
