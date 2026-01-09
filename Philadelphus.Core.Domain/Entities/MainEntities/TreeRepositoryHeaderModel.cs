using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Заголовок репозитория Чубушника
    /// </summary>
    public class TreeRepositoryHeaderModel : ITreeRepositoryHeaderModel
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid { get; set; }

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
        /// Время последнего открытия пользователем репозитория Чубушника
        /// </summary>
        public DateTime? LastOpening { get; set; }

        /// <summary>
        /// Избранный
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Скрытый
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Состояние
        /// </summary>
        public State State { get; internal set; } = State.Initialized;

        /// <summary>
        /// Заголовок репозитория Чубушника
        /// </summary>
        internal TreeRepositoryHeaderModel() 
        { 
        }
    }
}
