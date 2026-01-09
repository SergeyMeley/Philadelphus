namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Коллекция строк подключения
    /// </summary>
    public class ConnectionStringsCollection
    {
        /// <summary>
        /// Коллекция контейнеров строк подключения
        /// </summary>
        public IEnumerable<ConnectionStringContainer> ConnectionStringContainers { get; set; } = new List<ConnectionStringContainer>();
    }
}
