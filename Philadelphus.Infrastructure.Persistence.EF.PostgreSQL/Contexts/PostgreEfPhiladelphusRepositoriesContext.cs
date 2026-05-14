using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts
{
    /// <summary>
    /// Контекст данных для репозиториев Чубушника.
    /// </summary>
    public partial class PostgreEfPhiladelphusRepositoriesContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfPhiladelphusRepositoriesContext" />.
        /// </summary>
        public PostgreEfPhiladelphusRepositoriesContext()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfPhiladelphusRepositoriesContext" />.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        public PostgreEfPhiladelphusRepositoriesContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfPhiladelphusRepositoriesContext" />.
        /// </summary>
        /// <param name="options">Параметры конфигурации приложения.</param>
        public PostgreEfPhiladelphusRepositoriesContext(DbContextOptions<PostgreEfPhiladelphusRepositoriesContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Репозитории.
        /// </summary>
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
