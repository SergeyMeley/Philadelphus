using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts
{
    /// <summary>
    /// Контекст данных для SqliteEfPhiladelphusRepositoriesContext.
    /// </summary>
    public partial class SqliteEfPhiladelphusRepositoriesContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfPhiladelphusRepositoriesContext" />.
        /// </summary>
        public SqliteEfPhiladelphusRepositoriesContext()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfPhiladelphusRepositoriesContext" />.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        public SqliteEfPhiladelphusRepositoriesContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfPhiladelphusRepositoriesContext" />.
        /// </summary>
        /// <param name="options">Параметры конфигурации приложения.</param>
        public SqliteEfPhiladelphusRepositoriesContext(DbContextOptions<SqliteEfPhiladelphusRepositoriesContext> options)
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
