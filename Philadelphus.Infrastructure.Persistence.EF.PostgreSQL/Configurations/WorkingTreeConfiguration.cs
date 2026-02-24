using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Configurations
{
    public class WorkingTreeConfiguration : IEntityTypeConfiguration<WorkingTree>
    {
        public void Configure(EntityTypeBuilder<WorkingTree> builder)
        {
            builder.ToTable("working_trees", "shrub_members");

            builder.HasKey(x => x.Uuid).HasName("working_trees_pkey");

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

            builder.Property(x => x.Sequence)
                .HasColumnName("sequence");

            builder.Property(x => x.Alias)
                .HasColumnName("alias");

            builder.Property(x => x.CustomCode)
                .HasColumnName("custom_code");

            builder.Property(x => x.IsHidden)
                .HasColumnName("is_hidden")
                .IsRequired()
                .HasDefaultValue(false);

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
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("created_by")
                    .IsRequired();

                audit.Property(a => a.UpdatedAt)
                    .HasColumnName("updated_at");

                audit.Property(a => a.UpdatedBy)
                    .HasColumnName("updated_by");

                audit.Property(a => a.DeletedAt)
                    .HasColumnName("deleted_at");

                audit.Property(a => a.DeletedBy)
                    .HasColumnName("deleted_by");
            });
        }
    }
}
