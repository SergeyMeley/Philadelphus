using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreEfRepository.Contexts
{
    public partial class TreeRepositoryHeadersPhiladelphusContext : DbContext
    {
        private readonly string _connectionString;
        public TreeRepositoryHeadersPhiladelphusContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public TreeRepositoryHeadersPhiladelphusContext(DbContextOptions<TreeRepositoryHeadersPhiladelphusContext> options)
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
            modelBuilder.Entity<AuditInfo>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("audit_info_pkey");

                entity.ToTable("audit_info");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RepositoryElementId).HasColumnName("repository_element_id");
            });

            modelBuilder.Entity<TreeRepository>(entity =>
            {
                entity.ToTable("repositories");
                entity.HasBaseType<EntityBase>();

                //entity.Property(e => e.ChildTreeRootGuids).HasColumnName("root_uuids");
                entity.Property(e => e.Guid).HasColumnName("uuid");
            });

            modelBuilder.Entity<EntityBase>(entity =>
            {
                entity.ToTable("repository_elements");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Guid).HasColumnName("uuid");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
