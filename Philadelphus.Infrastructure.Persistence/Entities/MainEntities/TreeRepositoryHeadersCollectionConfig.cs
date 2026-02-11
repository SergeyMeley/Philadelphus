using System.ComponentModel;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    [DisplayName("Конфигурация заголовков репозиториев")]
    public class PhiladelphusRepositoryHeadersCollectionConfig
    {
        public List<PhiladelphusRepositoryHeader> PhiladelphusRepositoryHeaders { get; set; } = new List<PhiladelphusRepositoryHeader>();
    }
}
