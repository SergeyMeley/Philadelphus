using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Контейнер строки подключения
    /// </summary>
    public class ConnectionStringsContainer
    {
        /// <summary>
        /// Уникальный идентификатор хранилища для сопоставления
        /// </summary>
        public Guid StorageUuid { get; set; }

        /// <summary>
        /// Наименование провайдера БД
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Наименование провайдера БД
        /// </summary>
        public InfrastructureTypes InfrastructureType { get; set; }

        /// <summary>
        /// Строки подключения к БД для разных контекстов
        /// </summary>
        public Dictionary<InfrastructureEntityGroups, string> ConnectionStrings { get; set; }
    }
}
