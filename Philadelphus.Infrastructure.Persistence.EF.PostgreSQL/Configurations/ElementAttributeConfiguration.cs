using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Configurations
{
    public class ElementAttributeConfiguration : IEntityTypeConfiguration<ElementAttribute>
    {
        public void Configure(EntityTypeBuilder<ElementAttribute> builder)
        {
            builder.ToTable("element_attributes", "shrub_members_content");

            builder.HasKey(x => x.Uuid).HasName("element_attributes_pkey");

            builder.Property(x => x.Uuid)
                .HasColumnName("uuid")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.DeclaringUuid)
                .HasColumnName("declaring_uuid")
                .IsRequired();

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

            builder.Property(x => x.OwnerUuid)
                .HasColumnName("owner_uuid")
                .IsRequired();

            builder.Property(x => x.DeclaringOwnerUuid)
                .HasColumnName("declaring_owner_uuid")
                .IsRequired();

            builder.Property(x => x.ValueTypeUuid)
                .HasColumnName("value_type_uuid");

            builder.Property(x => x.ValueUuid)
                .HasColumnName("value_uuid");

            builder.Property(x => x.VisibilityId)
                .HasColumnName("visibility_id")
                .HasDefaultValue(0);

            builder.Property(x => x.OverrideId)
                .HasColumnName("override_id")
                .HasDefaultValue(0);
        }
    }
}
