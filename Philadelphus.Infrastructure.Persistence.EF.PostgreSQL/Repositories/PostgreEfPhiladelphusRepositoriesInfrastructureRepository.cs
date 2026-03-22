using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System.Diagnostics;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    public class PostgreEfPhiladelphusRepositoriesInfrastructureRepository : PostgreEfInfrastructureRepositoryBase<PhiladelphusRepositoriesContext>, IPhiladelphusRepositoriesInfrastructureRepository
    {
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.PhiladelphusRepositories; }
        public PostgreEfPhiladelphusRepositoriesInfrastructureRepository(
            ILogger logger,
            string connectionString,
            bool needEnsureDeleted = false)
            : base(logger, connectionString)
        {
        }
        protected override PhiladelphusRepositoriesContext GetNewContext() => new PhiladelphusRepositoriesContext(_connectionString);
        protected override DbSet<TEntity> GetDbSet<TEntity>(PhiladelphusRepositoriesContext context) where TEntity : class
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
