using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    /// <summary>
    /// Задает контракт для работы с репозиторем БД репозиториев Чубушника.
    /// </summary>
    public class PostgreEfPhiladelphusRepositoriesInfrastructureRepository : PostgreEfInfrastructureRepositoryBase<PostgreEfPhiladelphusRepositoriesContext>, IPhiladelphusRepositoriesInfrastructureRepository
    {
        /// <summary>
        /// Группа инфраструктурных сущностей.
        /// </summary>
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.PhiladelphusRepositories; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfPhiladelphusRepositoriesInfrastructureRepository" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="connectionString">Строка подключения.</param>
        public PostgreEfPhiladelphusRepositoriesInfrastructureRepository(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }

        protected override PostgreEfPhiladelphusRepositoriesContext GetNewContext() => new PostgreEfPhiladelphusRepositoriesContext(_connectionString);

        protected override DbSet<TEntity> GetDbSet<TEntity>(PostgreEfPhiladelphusRepositoriesContext context) where TEntity : class
        {
            return typeof(TEntity).Name switch
            {
                nameof(PhiladelphusRepository) => context.Repositories as DbSet<TEntity>,
                _ => throw new NotSupportedException($"Тип {typeof(TEntity).Name} не поддерживается.")
            };
        }

        /// <summary>
        /// Выполняет операцию SelectRepositories.
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы.</param>
        /// <returns>Коллекция полученных данных.</returns>
        public IEnumerable<PhiladelphusRepository> SelectRepositories(Guid[] uuids = null)
            => Select<PhiladelphusRepository>(ownUuids: uuids);

        /// <summary>
        /// Выполняет операцию репозитория.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertRepository(PhiladelphusRepository item)
            => Insert(new List<PhiladelphusRepository>() { item });

        /// <summary>
        /// Обновляет данные репозитория.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateRepository(PhiladelphusRepository item)
            => Update(new List<PhiladelphusRepository>() { item });

        /// <summary>
        /// Выполняет операцию репозитория.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteRepository(PhiladelphusRepository item)
            => SoftDelete(new List<PhiladelphusRepository>() { item });
    }
}
