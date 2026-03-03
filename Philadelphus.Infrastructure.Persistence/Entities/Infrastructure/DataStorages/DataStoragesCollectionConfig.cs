using System.ComponentModel;

namespace Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages
{
    [DisplayName("Конфигурация хранилищ данных")]
    public class DataStoragesCollectionConfig   // TODO: Перенести в Core?
    {
        public List<DataStorage> DataStorages { get; set; } = new List<DataStorage>();
    }
}
