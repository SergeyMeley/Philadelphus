using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Philadelphus.PostgreEfRepository.Contexts
{
    public partial class MainEntitiesPhiladelphusContext : DbContext
    {
        private readonly string _connectionString;
        public MainEntitiesPhiladelphusContext()
        {
        }
        public MainEntitiesPhiladelphusContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public MainEntitiesPhiladelphusContext(DbContextOptions<MainEntitiesPhiladelphusContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ElementAttribute> Attributes { get; set; }

        public virtual DbSet<TreeElementAttributeValue> AttributeValues { get; set; }

        //public virtual DbSet<AttributeValueGroup> AttributeValueGroups { get; set; }

        public virtual DbSet<AuditInfo> AuditInfos { get; set; }

        public virtual DbSet<TreeLeave> LeaveDetails { get; set; }

        //public virtual DbSet<LinkAttributeStringValueAndAttributeValueGroup> LinkAttributeStringValueAndAttributeValueGroups { get; set; }

        public virtual DbSet<TreeNode> NodeDetails { get; set; }

        public virtual DbSet<TreeRepository> Repositories { get; set; }

        public virtual DbSet<MainEntityBase> RepositoryElements { get; set; }

        public virtual DbSet<TreeRoot> RootDetails { get; set; }

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
            //modelBuilder.Entity<ElementAttribute>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("attributes_pkey");

            //    entity.ToTable("attributes");

            //    entity.Property(e => e.Id).HasColumnName("id");
            //    entity.Property(e => e.ParentGuid).HasColumnName("data_type_node_uuid");
            //    entity.Property(e => e.ParentGuid).HasColumnName("repository_element_id");
            //    entity.Property(e => e.Guid).HasColumnName("uuid");
            //});

            //modelBuilder.Entity<AttributeValue>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("attribute_string_values_pkey");

            //    entity.ToTable("attribute_string_values");

            //    entity.Property(e => e.Id).HasColumnName("id");
            //    entity.Property(e => e.ParentGuid).HasColumnName("data_type_node_uuid");
            //    entity.Property(e => e.Guid).HasColumnName("uuid");
            //});

            //modelBuilder.Entity<AttributeValueGroup>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("attribute_value_groups_pkey");

            //    entity.ToTable("attribute_value_groups");

            //    entity.Property(e => e.Id).HasColumnName("id");
            //    entity.Property(e => e.DataTypeNodeUuid).HasColumnName("data_type_node_uuid");
            //    entity.Property(e => e.Uuid).HasColumnName("uuid");
            //});

            modelBuilder.Entity<AuditInfo>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("audit_info_pkey");

                entity.ToTable("audit_info");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RepositoryElementUuid).HasColumnName("repository_element_id");
            });

            modelBuilder.Entity<TreeLeave>(entity =>
            {
                entity.ToTable("leave_details");
                entity.HasBaseType<MainEntityBase>();

                entity.Property(e => e.Id).HasColumnName("repository_element_id");
            });

            //modelBuilder.Entity<LinkAttributeStringValueAndAttributeValueGroup>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("link_attribute_string_value_and_attribute_value_group_pkey");

            //    entity.ToTable("link_attribute_string_value_and_attribute_value_group");

            //    entity.Property(e => e.Id).HasColumnName("id");
            //    entity.Property(e => e.AttributeValueGroupUuid).HasColumnName("attribute_value_group_uuid");
            //    entity.Property(e => e.AttributeValueUuid).HasColumnName("attribute_value_uuid");
            //    entity.Property(e => e.Uuid).HasColumnName("uuid");
            //});

            modelBuilder.Entity<TreeNode>(entity =>
            {
                entity.ToTable("node_details");
                entity.HasBaseType<MainEntityBase>();

                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Id).HasColumnName("repository_element_id");
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

            modelBuilder.Entity<TreeRoot>(entity =>
            {
                entity.ToTable("root_details");
                entity.HasBaseType<MainEntityBase>();

                entity.Property(e => e.Id).HasColumnName("repository_element_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
