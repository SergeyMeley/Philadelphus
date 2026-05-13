namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    /// <summary>
    /// Представляет объект заголовка репозитория Чубушника.
    /// </summary>
    public class PhiladelphusRepositoryHeader
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public Guid Uuid { get; set; }
       
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }
       
        /// <summary>
        /// Описание.
        /// </summary>
        public string? Description { get; set; }
      
        /// <summary>
        /// Наименование собственного хранилища данных.
        /// </summary>
        public string OwnDataStorageName { get; set; }

        /// <summary>
        /// Уникальный идентификатор собственного хранилища данных.
        /// </summary>
        public Guid OwnDataStorageUuid { get; set; }
       
        /// <summary>
        /// Время последнего открытия.
        /// </summary>
        public DateTime? LastOpening { get; set; }
       
        /// <summary>
        /// Признак избранного элемента.
        /// </summary>
        public bool IsFavorite { get; set; }
      
        /// <summary>
        /// Признак скрытого элемента.
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
