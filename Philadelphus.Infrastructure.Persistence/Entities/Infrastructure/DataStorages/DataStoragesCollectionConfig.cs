using System.ComponentModel;

namespace Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages
{
    /// <summary>
    /// Конфигурация коллекции хранилищ данных.
    /// </summary>
    [DisplayName("Конфигурация хранилищ данных")]
    public class DataStoragesCollectionConfig   // TODO: Перенести в Core?
    {
        /// <summary>
        /// Коллекция хранилищ данных.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public List<DataStorage> DataStorages { get; set; } = new List<DataStorage>();
    }
}
