using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts
{
    /// <summary>
    /// Контекст данных для отчета.
    /// </summary>
    public partial class PostgreEfReportsContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfReportsContext" />.
        /// </summary>
        public PostgreEfReportsContext()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfReportsContext" />.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        public PostgreEfReportsContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfReportsContext" />.
        /// </summary>
        /// <param name="options">Параметры конфигурации приложения.</param>
        public PostgreEfReportsContext(DbContextOptions<PostgreEfReportsContext> options)
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
                    .UseNpgsql(_connectionString);
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
