using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts
{
    public class PostgreEfReportsContext : DbContext
    {
        private readonly string _connectionString;
        public PostgreEfReportsContext()
        {
        }
        public PostgreEfReportsContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public PostgreEfReportsContext(DbContextOptions<PostgreEfReportsContext> options)
            : base(options) 
        { 
        }

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
        }
    }
}
