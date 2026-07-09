using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages
{
    /// <summary>
    /// Представляет объект хранилища данных.
    /// </summary>
    public class DataStorage
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
        public string Description { get; set; }

        /// <summary>
        /// Тип инфраструктуры.
        /// </summary>
        public InfrastructureTypes InfrastructureType { get; set; }

        /// <summary>
        /// Наименование провайдера БД.
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Строки подключения к БД для разных групп сущностей.
        /// </summary>
        public Dictionary<InfrastructureEntityGroups, string> ConnectionStrings { get; set; } = new();

        /// <summary>
        /// Указывает наличие репозитория БД инфраструктурного репозитория.
        /// </summary>
        public bool HasPhiladelphusRepositoriesInfrastructureRepository { get; set; }

        /// <summary>
        /// Указывает наличие репозитория БД участника кустарника.
        /// </summary>
        public bool HasShrubMembersInfrastructureRepository { get; set; }

        /// <summary>
        /// Указывает наличие репозитория БД отчета.
        /// </summary>
        public bool HasReportsInfrastructureRepository { get; set; }

        /// <summary>
        /// Признак отключенного хранилища. Отключенные хранилища загружаются без инфраструктурных репозиториев.
        /// </summary>
        public bool? IsDisabled { get; set; } = null;
        
        /// <summary>
        /// Признак скрытого элемента.
        /// </summary>
        public bool IsHidden { get; set; } = false;
    }
}
