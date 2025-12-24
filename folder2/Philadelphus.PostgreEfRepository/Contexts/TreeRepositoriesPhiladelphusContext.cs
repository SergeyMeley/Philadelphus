using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.PostgreEfRepository.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreEfRepository.Contexts
{
    public partial class TreeRepositoriesPhiladelphusContext : DbContext
    {
        private readonly string _connectionString;
        public TreeRepositoriesPhiladelphusContext()
        {
        }
        public TreeRepositoriesPhiladelphusContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public TreeRepositoriesPhiladelphusContext(DbContextOptions<TreeRepositoriesPhiladelphusContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TreeRepository> Repositories { get; set; }

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
            modelBuilder.ApplyConfiguration(new TreeRepositoryConfiguration());
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
