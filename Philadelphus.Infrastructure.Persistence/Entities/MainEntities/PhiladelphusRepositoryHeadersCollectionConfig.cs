using System.ComponentModel;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    /// <summary>
    /// Конфигурация коллекции заголовков репозиториев Чубушника.
    /// </summary>
    [DisplayName("Конфигурация заголовков репозиториев")]
    public class PhiladelphusRepositoryHeadersCollectionConfig  // TODO: Перенести в Core?
    {
        /// <summary>
        /// Заголовки репозиториев.
        /// </summary>
        public List<PhiladelphusRepositoryHeader> PhiladelphusRepositoryHeaders { get; set; } = new List<PhiladelphusRepositoryHeader>();
    }
}
