using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreEfRepository.Configurations
{
    public class TreeRepositoryConfiguration : IEntityTypeConfiguration<TreeRepository>
    {
        public void Configure(EntityTypeBuilder<TreeRepository> builder)
        {
            builder.ToTable("tree_repositories", "repositories");

            builder.HasKey(x => x.Guid).HasName("tree_repositories_pkey");

            builder.Property(x => x.Guid)
                .HasColumnName("uuid")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.OwnDataStorageGuid)
                .HasColumnName("data_storage_uuid")
                  .IsRequired();

            builder.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.IsDeleted)
                    .HasColumnName("is_deleted")
                    .IsRequired()
                    .HasDefaultValue(false);

                audit.Property(a => a.CreatedOn)
                    .HasColumnName("created_at")
                    .IsRequired()
                    .HasMaxLength(50);

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("created_by")
                    .IsRequired()
                    .HasMaxLength(100);

                audit.Property(a => a.UpdatedOn)
                    .HasColumnName("updated_at")
                    .IsRequired()
                    .HasMaxLength(50);

                audit.Property(a => a.UpdatedBy)
                    .HasColumnName("updated_by")
                    .IsRequired()
                    .HasMaxLength(100);

                audit.Property(a => a.UpdatedContentOn)
                    .HasColumnName("content_updated_at")
                    .HasMaxLength(50);

                audit.Property(a => a.UpdatedContentBy)
                    .HasColumnName("content_updated_by")
                    .HasMaxLength(100);

                audit.Property(a => a.DeletedOn)
                    .HasColumnName("deleted_at")
                    .HasMaxLength(50);

                audit.Property(a => a.DeletedBy)
                    .HasColumnName("deleted_by")
                    .HasMaxLength(100);
            });

            builder.Property(x => x.ChildTreeRootsGuids)
                .HasColumnName("child_tree_roots_uuids")
                .HasColumnType("uuid[]");

            builder.Property(x => x.DataStoragesGuids)
                .HasColumnName("data_storages_uuids")
                .HasColumnType("uuid[]");

        }
    }
}
