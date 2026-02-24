using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Configurations
{
    public class TreeLeaveConfiguration : IEntityTypeConfiguration<TreeLeave>
    {
        public void Configure(EntityTypeBuilder<TreeLeave> builder)
        {
            builder.ToTable("tree_leaves", "shrub_members");

            builder.HasKey(x => x.Uuid).HasName("tree_leaves_pkey");

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

                audit.Property(a => a.DeletedAt)
                    .HasColumnName("deleted_at");

                audit.Property(a => a.DeletedBy)
                    .HasColumnName("deleted_by");
            });

            builder.Property(p => p.SystemBaseTypeId)
                .HasColumnName("data_type_id")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.OwningWorkingTreeUuid)
                .HasColumnName("owning_working_tree_uuid")
                .IsRequired();

            builder.Property(x => x.ParentTreeNodeUuid)
                .HasColumnName("parent_tree_node_uuid");

            builder.HasOne(x => x.OwningWorkingTree)
              .WithMany()
              .HasForeignKey(x => x.OwningWorkingTreeUuid);

            builder.HasOne(x => x.ParentTreeNode)
                  .WithMany()
                  .HasForeignKey(x => x.ParentTreeNodeUuid);
        }
    }
}
