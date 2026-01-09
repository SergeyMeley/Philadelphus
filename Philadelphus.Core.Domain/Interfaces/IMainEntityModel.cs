using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Основная сущность
    /// </summary>
    public interface IMainEntityModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Тип сущности (устар.)
        /// </summary>
        public abstract EntityTypesModel EntityType { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Псевдоним. Аникален в рамках корня дерева сущностей Чубушника
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Пользовательский код. Аникален в рамках корня дерева сущностей Чубушника
        /// </summary>
        public string CustomCode { get; set; }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        public AuditInfoModel AuditInfo { get; }

        /// <summary>
        /// Состояние
        /// </summary>
        public State State { get; }

        /// <summary>
        /// Сущность БД
        /// </summary>
        public IMainEntity DbEntity { get; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public IDataStorageModel DataStorage { get; }
    }
}
