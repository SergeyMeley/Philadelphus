using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Philadelphus.PostgreEfRepository
{
    public partial class PhiladelphusMainContext : DbContext
    {
        public PhiladelphusMainContext()
        {
        }

        public PhiladelphusMainContext(DbContextOptions<PhiladelphusMainContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ElementAttribute> Attributes { get; set; }

        public virtual DbSet<AttributeValue> AttributeValues { get; set; }

        //public virtual DbSet<AttributeValueGroup> AttributeValueGroups { get; set; }

        public virtual DbSet<AuditInfo> AuditInfos { get; set; }

        public virtual DbSet<LeaveDetail> LeaveDetails { get; set; }

        public virtual DbSet<LinkAttributeStringValueAndAttributeValueGroup> LinkAttributeStringValueAndAttributeValueGroups { get; set; }

        public virtual DbSet<NodeDetail> NodeDetails { get; set; }

        public virtual DbSet<OtherSchemaTest> OtherSchemaTests { get; set; }

        public virtual DbSet<Repository> Repositories { get; set; }

        public virtual DbSet<RepositoryElement> RepositoryElements { get; set; }

        public virtual DbSet<RootDetail> RootDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseNpgsql("Host=192.168.0.63;Port=5433;Username=postgres;Password=Dniwe2002;Database=philadelphus");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attribute>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("attributes_pkey");

                entity.ToTable("attributes");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DataTypeNodeUuid).HasColumnName("data_type_node_uuid");
                entity.Property(e => e.RepositoryElementId).HasColumnName("repository_element_id");
                entity.Property(e => e.Uuid).HasColumnName("uuid");
            });

            modelBuilder.Entity<AttributeStringValue>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("attribute_string_values_pkey");

                entity.ToTable("attribute_string_values");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DataTypeNodeUuid).HasColumnName("data_type_node_uuid");
                entity.Property(e => e.Uuid).HasColumnName("uuid");
            });

            modelBuilder.Entity<AttributeValueGroup>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("attribute_value_groups_pkey");

                entity.ToTable("attribute_value_groups");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DataTypeNodeUuid).HasColumnName("data_type_node_uuid");
                entity.Property(e => e.Uuid).HasColumnName("uuid");
            });

            modelBuilder.Entity<AuditInfo>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("audit_info_pkey");

                entity.ToTable("audit_info");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RepositoryElementId).HasColumnName("repository_element_id");
            });

            modelBuilder.Entity<LeaveDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("leave_details_pkey");

                entity.ToTable("leave_details");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RepositoryElementId).HasColumnName("repository_element_id");
            });

            modelBuilder.Entity<LinkAttributeStringValueAndAttributeValueGroup>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("link_attribute_string_value_and_attribute_value_group_pkey");

                entity.ToTable("link_attribute_string_value_and_attribute_value_group");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AttributeValueGroupUuid).HasColumnName("attribute_value_group_uuid");
                entity.Property(e => e.AttributeValueUuid).HasColumnName("attribute_value_uuid");
                entity.Property(e => e.Uuid).HasColumnName("uuid");
            });

            modelBuilder.Entity<NodeDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("node_details_pkey");

                entity.ToTable("node_details");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.RepositoryElementId).HasColumnName("repository_element_id");
            });

            modelBuilder.Entity<OtherSchemaTest>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("other_schema_test_pkey");

                entity.ToTable("other_schema_test", "test");

                entity.Property(e => e.Id).HasColumnName("id");
            });

            modelBuilder.Entity<TreeRepository>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("repositories_pkey");

                entity.ToTable("repositories");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ChildTreeRootGuids).HasColumnName("root_uuids");
                entity.Property(e => e.Guid).HasColumnName("uuid");
            });

            modelBuilder.Entity<RepositoryElement>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("repository_elements_pkey");

                entity.ToTable("repository_elements");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Uuid).HasColumnName("uuid");
            });

            modelBuilder.Entity<RootDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("root_details_pkey");

                entity.ToTable("root_details");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RepositoryElementId).HasColumnName("repository_element_id");
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("test_pkey");

                entity.ToTable("test");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Test1>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("test_pkey");

                entity.ToTable("test", "test");

                entity.Property(e => e.Id).HasColumnName("id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
