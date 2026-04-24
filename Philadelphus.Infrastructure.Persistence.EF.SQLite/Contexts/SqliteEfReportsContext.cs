using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts
{
    public partial class SqliteEfReportsContext : DbContext
    {
        private readonly string _connectionString;
        public SqliteEfReportsContext()
        {
        }
        public SqliteEfReportsContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public SqliteEfReportsContext(DbContextOptions<SqliteEfReportsContext> options)
            : base(options)
        {
        }

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