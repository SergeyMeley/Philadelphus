using System.ComponentModel;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    [DisplayName("Конфигурация заголовков репозиториев")]
    public class TreeRepositoryHeadersCollectionConfig
    {
        public List<TreeRepositoryHeader> TreeRepositoryHeaders { get; set; } = new List<TreeRepositoryHeader>();
    }
}
