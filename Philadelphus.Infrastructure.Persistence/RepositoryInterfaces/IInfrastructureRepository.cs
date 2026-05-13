using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    /// <summary>
    /// Задает контракт для работы с репозиторем БД.
    /// </summary>
    public interface IInfrastructureRepository
    {
        /// <summary>
        /// Группа инфраструктурных сущностей.
        /// </summary>
        public InfrastructureEntityGroups EntityGroup { get; }
        
        /// <summary>
        /// Проверить доступность репозитория БД.
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CheckAvailability();
    }
}
