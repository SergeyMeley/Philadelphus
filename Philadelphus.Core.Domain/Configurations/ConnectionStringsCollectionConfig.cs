using System.ComponentModel;

namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Коллекция строк подключения
    /// </summary>
    [DisplayName("Конфигурация строк подключения")]
    public class ConnectionStringsCollectionConfig
    {
        /// <summary>
        /// Коллекция контейнеров строк подключения
        /// </summary>
        public List<ConnectionStringContainer> ConnectionStringContainers { get; set; } = new List<ConnectionStringContainer>();
    }
}
