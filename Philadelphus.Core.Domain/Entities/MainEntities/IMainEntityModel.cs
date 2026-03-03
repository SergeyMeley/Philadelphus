using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Основная сущность
    /// </summary>
    public interface IMainEntityModel : ILinkableByUuidModel
    {
        #region [ Properties ]

        #region [ General Properties ]

        /// <summary>
        /// Наименование
        /// Уникально в рамках дерева сущностей
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        public AuditInfoModel AuditInfo { get; }

        /// <summary>
        /// Скрыт от нового использования (устаревший для бизнеса)
        /// </summary>
        public bool IsHidden { get; set; }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Состояние
        /// </summary>
        public State State { get; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public IDataStorageModel DataStorage { get; }

        #endregion

        #endregion

        #region [ Methods ]



        #endregion

        #region OLD     

        /// <summary>
        /// Сущность БД
        /// </summary>
        public IMainEntity DbEntity { get; }    // TODO: Исключить и применить автомаппер

        #endregion
    }
}
