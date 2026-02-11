using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts
{
    public partial class PhiladelphusRepositoriesPhiladelphusContext : DbContext
    {
        private readonly string _connectionString;
        public PhiladelphusRepositoriesPhiladelphusContext()
        {
        }
        public PhiladelphusRepositoriesPhiladelphusContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public PhiladelphusRepositoriesPhiladelphusContext(DbContextOptions<PhiladelphusRepositoriesPhiladelphusContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PhiladelphusRepository> Repositories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseNpgsql(_connectionString)
                    .UseLazyLoadingProxies();
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
