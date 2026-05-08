using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    public class PostgreEfPhiladelphusRepositoriesInfrastructureRepository : PostgreEfInfrastructureRepositoryBase<PostgreEfPhiladelphusRepositoriesContext>, IPhiladelphusRepositoriesInfrastructureRepository
    {
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.PhiladelphusRepositories; }

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
