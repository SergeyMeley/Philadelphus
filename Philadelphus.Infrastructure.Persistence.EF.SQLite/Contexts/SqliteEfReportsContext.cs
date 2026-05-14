using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts
{
    /// <summary>
    /// Контекст данных для отчета.
    /// </summary>
    public partial class SqliteEfReportsContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfReportsContext" />.
        /// </summary>
        public SqliteEfReportsContext()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfReportsContext" />.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        public SqliteEfReportsContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfReportsContext" />.
        /// </summary>
        /// <param name="options">Параметры конфигурации приложения.</param>
        public SqliteEfReportsContext(DbContextOptions<SqliteEfReportsContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Динамические результаты.
        /// </summary>
        public DbSet<DynamicReportResult> DynamicResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlite(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DynamicReportResult>()
                .HasNoKey()
                .ToView(null); // Без таблицы, только для запросов

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}