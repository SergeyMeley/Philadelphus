using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts
{
    public partial class PhiladelphusRepositoriesContext : DbContext
    {
        private readonly string _connectionString;
        public PhiladelphusRepositoriesContext()
        {
        }
        public PhiladelphusRepositoriesContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public PhiladelphusRepositoriesContext(DbContextOptions<PhiladelphusRepositoriesContext> options)
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
                    .UseLazyLoadingProxies()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    //.LogTo(Log.Information, LogLevel.Information)
                    //.EnableSensitiveDataLogging()
                    //.EnableDetailedErrors();
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
