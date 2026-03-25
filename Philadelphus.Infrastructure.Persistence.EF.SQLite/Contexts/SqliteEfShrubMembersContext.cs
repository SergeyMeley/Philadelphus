using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts
{
    public partial class SqliteEfShrubMembersContext : DbContext
    {
        private readonly string _connectionString;
        public SqliteEfShrubMembersContext()
        {
        }
        public SqliteEfShrubMembersContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public SqliteEfShrubMembersContext(DbContextOptions<SqliteEfShrubMembersContext> options)
            : base(options)
        {
        }

        public virtual DbSet<WorkingTree> WorkingTrees { get; set; }
        public virtual DbSet<TreeRoot> TreeRoots { get; set; }
        public virtual DbSet<TreeNode> TreeNodes { get; set; }
        public virtual DbSet<TreeLeave> TreeLeaves { get; set; }
        public virtual DbSet<ElementAttribute> ElementAttributes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlite(_connectionString)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<WorkingTreeMemberBase>();
            modelBuilder.ApplyConfiguration(new WorkingTreeConfiguration());
            modelBuilder.ApplyConfiguration(new TreeRootConfiguration());
            modelBuilder.ApplyConfiguration(new TreeNodeConfiguration());
            modelBuilder.ApplyConfiguration(new TreeLeaveConfiguration());
            modelBuilder.ApplyConfiguration(new ElementAttributeConfiguration());
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
