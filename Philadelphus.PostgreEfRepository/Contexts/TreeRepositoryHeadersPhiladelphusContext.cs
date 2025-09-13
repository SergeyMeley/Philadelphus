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
                entity.HasKey(e => e.Id).HasName("audit_infos_pkey");

                entity.ToTable("audit_infos");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RepositoryElementId).HasColumnName("repository_element_id");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedOn).HasColumnName("created_on");
                entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
                entity.Property(e => e.DeletedOn).HasColumnName("deleted_on");
                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
                entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
                entity.Property(e => e.UpdatedContentBy).HasColumnName("updated_content_by");
                entity.Property(e => e.UpdatedContentOn).HasColumnName("updated_content_on");
                entity.Property(e => e.UpdatedOn).HasColumnName("updated_on");

            });

            modelBuilder.Entity<TreeRepository>(entity =>
            {
                entity.ToTable("repositories");
                entity.HasBaseType<MainEntityBase>();

                //entity.Property(e => e.ChildTreeRootGuids).HasColumnName("root_uuids");
                entity.Property(e => e.Guid).HasColumnName("uuid");
            });

            modelBuilder.Entity<MainEntityBase>(entity =>
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
