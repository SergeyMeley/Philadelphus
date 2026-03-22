using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages
{
    /// <summary>
    /// Хранилище данных  
    /// </summary>
    public interface IDataStorageModel
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
        public string Description { get; set; }

        /// <summary>
        /// Тип хранилища данных
        /// </summary>
        public InfrastructureTypes InfrastructureType { get; }

        /// <summary>
        /// Репозитории БД
        /// </summary>
        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories { get; }

        /// <summary>
        /// Репозиторий БД работы с репозиториями Чубушника
        /// </summary>
        public IPhiladelphusRepositoriesInfrastructureRepository PhiladelphusRepositoriesInfrastructureRepository { get; }

        /// <summary>
        /// Репозиторий БД работы с участниками кустарника репозитория Чубушника
        /// </summary>
        public IShrubMembersInfrastructureRepository ShrubMembersInfrastructureRepository { get; }

        /// <summary>
        /// Имеет репозиторий БД работы с репозиториями Чубушника 
        /// </summary>
        public bool HasPhiladelphusRepositoriesInfrastructureRepository { get; }

        /// <summary>
        /// Имеет репозиторий БД работы с участниками кустарника репозитория Чубушника
        /// </summary>
        public bool HasShrubMembersInfrastructureRepository { get; }

        /// <summary>
        /// Доступность хранилища данных
        /// </summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// Состояние отключенности
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Время последней проверки доступности
        /// </summary>
        public DateTime LastCheckTime { get; }

        /// <summary>
        /// Проверить доступность
        /// </summary>
        /// <returns></returns>
        public bool CheckAvailable();

        /// <summary>
        /// Проверить доступность
        /// </summary>
        /// <returns></returns>
        public Task<bool> CheckAvailableAsync();

        /// <summary>
        /// Запустить автоматическую проверку доступности
        /// </summary>
        /// <param name="interval">Интервал проверки (сек.). Не рекомендуется устанавливать период менее 60 сек.</param>
        /// <returns></returns>
        public bool StartAvailableAutoChecking(int interval);

        /// <summary>
        /// Остановить автоматическую проверку доступности
        /// </summary>
        /// <returns></returns>
        public bool StopAvailableAutoChecking();
    }
}
