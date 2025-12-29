using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Configurations
{
    public class TreeRepositoryConfiguration : IEntityTypeConfiguration<TreeRepository>
    {
        public void Configure(EntityTypeBuilder<TreeRepository> builder)
        {
            builder.ToTable("tree_repositories", "repositories");

            builder.HasKey(x => x.Uuid).HasName("tree_repositories_pkey");

            builder.Property(x => x.Uuid)
                .HasColumnName("uuid")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description");

            builder.Property(x => x.OwnDataStorageUuid)
                .HasColumnName("data_storage_uuid")
                  .IsRequired();

            builder.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.IsDeleted)
                    .HasColumnName("is_deleted")
                    .IsRequired()
                    .HasDefaultValue(false);

                audit.Property(a => a.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("created_by")
                    .IsRequired()
                    .HasDefaultValue("session_user");

                audit.Property(a => a.UpdatedAt)
                    .HasColumnName("updated_at");

                audit.Property(a => a.UpdatedBy)
                    .HasColumnName("updated_by");

                audit.Property(a => a.ContentUpdatedAt)
                    .HasColumnName("content_updated_at");

                audit.Property(a => a.ContentUpdatedBy)
                    .HasColumnName("content_updated_by");

                audit.Property(a => a.DeletedAt)
                    .HasColumnName("deleted_at");

                audit.Property(a => a.DeletedBy)
                    .HasColumnName("deleted_by");
            });

            builder.Property(x => x.ChildTreeRootsUuids)
                .HasColumnName("child_tree_roots_uuids")
                .HasColumnType("uuid[]");

            builder.Property(x => x.DataStoragesUuids)
                .HasColumnName("data_storages_uuids")
                .HasColumnType("uuid[]");

        }
    }
}
