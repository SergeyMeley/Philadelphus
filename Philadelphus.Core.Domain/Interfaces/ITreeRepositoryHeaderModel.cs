using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Заголовок репозитория
    /// </summary>
    public interface ITreeRepositoryHeaderModel
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Наименование собственного хранилища данных
        /// </summary>
        public string OwnDataStorageName { get; set; }

        /// <summary>
        /// Уникальный идентификатор собственного хранилища данных
        /// </summary>
        public Guid OwnDataStorageUuid { get; set; }

        /// <summary>
        /// Состояние
        /// </summary>
        public State State { get; }

        /// <summary>
        /// Последнее открытие
        /// </summary>
        public DateTime? LastOpening { get; set; }

        /// <summary>
        /// Избранный
        /// </summary>
        public bool IsFavorite { get; set; }
    }
}
