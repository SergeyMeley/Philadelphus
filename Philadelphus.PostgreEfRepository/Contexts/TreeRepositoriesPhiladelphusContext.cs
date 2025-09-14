using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.MainEntities;
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
            modelBuilder.Entity<AuditInfo>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("audit_infos_pkey");

                entity.ToTable("audit_infos", "general");

                entity.Property(e => e.Id).HasColumnName("id");
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
                entity.HasKey(e => e.Guid).HasName("tree_repositories_pkey");

                entity.ToTable("tree_repositories", "repositories");
                entity.HasBaseType<MainEntityBase>();

                entity.Property(e => e.Guid)
                    .ValueGeneratedNever()
                    .HasColumnName("guid");
                entity.Property(e => e.Alias).HasColumnName("alias");
                //entity.Property(e => e.AuditInfo.Id)
                //    .ValueGeneratedOnAdd()
                //    .HasColumnName("audit_info_id");
                entity.Property(e => e.CustomCode).HasColumnName("custom_code");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsLegacy).HasColumnName("is_legacy");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.OwnDataStorageGuid).HasColumnName("own_data_storage_guid");
            });

            modelBuilder.Entity<MainEntityBase>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("main_entity_base_pkey");

                entity.ToTable("main_entity", "general");

                entity.Property(e => e.Id)
                    //.HasDefaultValueSql("nextval('general.main_entity_base_id_seq'::regclass)")
                    .HasColumnName("id");
                entity.Property(e => e.Alias).HasColumnName("alias");
                //entity.Property(e => e.AuditInfo.Id).HasColumnName("audit_info_id");
                entity.Property(e => e.CustomCode).HasColumnName("custom_code");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsLegacy).HasColumnName("is_legacy");
                entity.Property(e => e.Name).HasColumnName("name");

                //entity.HasOne(d => d.AuditInfo).WithOne(p => p.RepositoryElementUuid)
                //    .HasForeignKey(d => d.AuditInfoId)
                //    .HasConstraintName("fkey_main_entities_audit_infos_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
