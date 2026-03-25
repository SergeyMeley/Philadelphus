using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories
{
    public class SqliteEfPhiladelphusRepositoriesInfrastructureRepository : SqliteEfInfrastructureRepositoryBase<SqliteEfPhiladelphusRepositoriesContext>, IPhiladelphusRepositoriesInfrastructureRepository
    {
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.PhiladelphusRepositories; }

        public SqliteEfPhiladelphusRepositoriesInfrastructureRepository(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }

        protected override SqliteEfPhiladelphusRepositoriesContext GetNewContext() => new SqliteEfPhiladelphusRepositoriesContext(_connectionString);

        protected override DbSet<TEntity> GetDbSet<TEntity>(SqliteEfPhiladelphusRepositoriesContext context) where TEntity : class
        {
            return typeof(TEntity).Name switch
            {
                nameof(PhiladelphusRepository) => context.Repositories as DbSet<TEntity>,
                _ => throw new NotSupportedException($"Тип {typeof(TEntity).Name} не поддерживается.")
            };
        }

        protected override void SetNavigationProperties<TEntity, TNav>(TEntity item, Dictionary<Guid, TNav> navigationEntities)
        {
            switch (item)
            {
                case PhiladelphusRepository repository:
                    break;
            }
        }

        public IEnumerable<PhiladelphusRepository> SelectRepositories(Guid[] uuids = null)
            => Select<PhiladelphusRepository>(ownUuids: uuids);

        public long InsertRepository(PhiladelphusRepository item)
            => Insert(new List<PhiladelphusRepository>() { item });

        public long UpdateRepository(PhiladelphusRepository item)
            => Update(new List<PhiladelphusRepository>() { item });

        public long SoftDeleteRepository(PhiladelphusRepository item)
            => SoftDelete(new List<PhiladelphusRepository>() { item });
    }
}
