using System.ComponentModel;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    [DisplayName("Конфигурация заголовков репозиториев")]
    public class PhiladelphusRepositoryHeadersCollectionConfig  // TODO: Перенести в Core?
    {
        public List<PhiladelphusRepositoryHeader> PhiladelphusRepositoryHeaders { get; set; } = new List<PhiladelphusRepositoryHeader>();
    }
}
