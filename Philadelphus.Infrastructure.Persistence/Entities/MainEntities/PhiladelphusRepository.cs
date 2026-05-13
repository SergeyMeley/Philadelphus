using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    /// <summary>
    /// Основной сервис работы с репозиторием Чубушника.
    /// </summary>
    public class PhiladelphusRepository : MainEntityBase
    {
        /// <summary>
        /// Собственное хранилище данных.
        /// </summary>
        public Guid OwnDataStorageUuid { get; set; }
       
        /// <summary>
        /// Рабочее дерево.
        /// </summary>
        public Guid[] ContentWorkingTreesUuids { get; set; }
       
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PhiladelphusRepository" />.
        /// </summary>
        public PhiladelphusRepository()
        {
            
        }
    }
}
