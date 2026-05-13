using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    /// <summary>
    /// Задает контракт для работы с репозиторем БД репозиториев Чубушника.
    /// </summary>
    public interface IPhiladelphusRepositoriesInfrastructureRepository : IInfrastructureRepository
    {
        /// <summary>
        /// Получить репозитории Чубушника.
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы.</param>
        /// <returns>Коллекция полученных данных.</returns>
        public IEnumerable<PhiladelphusRepository> SelectRepositories(Guid[] uuids = null);
        /// <summary>
        /// Добавить репозитории Чубушника.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertRepository(PhiladelphusRepository item);

        /// <summary>
        /// Обновить репозитории Чубушника.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateRepository(PhiladelphusRepository item);
       
        /// <summary>
        /// Удалить мягко репозитории Чубушника.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteRepository(PhiladelphusRepository item);
    }
}
