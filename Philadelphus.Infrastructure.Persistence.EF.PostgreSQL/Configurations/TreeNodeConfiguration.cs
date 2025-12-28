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
    public class TreeNodeConfiguration : IEntityTypeConfiguration<TreeNode>
    {
        public void Configure(EntityTypeBuilder<TreeNode> builder)
        {
            builder.ToTable("tree_nodes", "main_entities");

            builder.HasKey(x => x.Guid).HasName("tree_nodes_pkey");

            builder.Property(x => x.Guid)
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

            builder.Property(x => x.IsLegacy)
                .HasColumnName("is_legacy");

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

            builder.Property(x => x.ParentTreeRootGuid)
                .HasColumnName("parent_tree_root_uuid");

            builder.Property(x => x.ParentTreeNodeGuid)
                .HasColumnName("parent_tree_node_uuid");

            builder.Ignore(x => x.Parent);

            builder.HasOne(x => x.ParentTreeRoot)
              .WithMany()
              .HasForeignKey(x => x.ParentTreeRootGuid);

            //builder.HasOne(x => x.ParentTreeNode)
            //      .WithMany()
            //      .HasForeignKey(x => x.ParentGuid);

        }
    }
}
