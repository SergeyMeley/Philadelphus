using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts
{
    public partial class SqliteEfPhiladelphusRepositoriesContext : DbContext
    {
        private readonly string _connectionString;
        public SqliteEfPhiladelphusRepositoriesContext()
        {
        }
        public SqliteEfPhiladelphusRepositoriesContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public SqliteEfPhiladelphusRepositoriesContext(DbContextOptions<SqliteEfPhiladelphusRepositoriesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PhiladelphusRepository> Repositories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlite(_connectionString)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PhiladelphusRepositoryConfiguration());
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
