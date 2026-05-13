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
        /// Признак скрытого элемента.
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
