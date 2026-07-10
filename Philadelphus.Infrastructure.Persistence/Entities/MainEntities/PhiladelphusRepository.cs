using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.Common.Enums;

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
        /// Возможные хранилища данных для содержимого репозитория.
        /// </summary>
        public Guid[] AvailableDataStorageUuids { get; set; } = Array.Empty<Guid>();

        /// <summary>
        /// Хранилища данных по умолчанию для групп сущностей.
        /// </summary>
        public Dictionary<InfrastructureEntityGroups, Guid> DefaultDataStorageUuids { get; set; } = new();
       
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
