using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts
{
    /// <summary>
    /// Контекст данных для участника кустарника.
    /// </summary>
    public partial class SqliteEfShrubMembersContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfShrubMembersContext" />.
        /// </summary>
        public SqliteEfShrubMembersContext()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfShrubMembersContext" />.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        public SqliteEfShrubMembersContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SqliteEfShrubMembersContext" />.
        /// </summary>
        /// <param name="options">Параметры конфигурации приложения.</param>
        public SqliteEfShrubMembersContext(DbContextOptions<SqliteEfShrubMembersContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Рабочее дерево.
        /// </summary>
        public virtual DbSet<WorkingTree> WorkingTrees { get; set; }

        /// <summary>
        /// Корень рабочего дерева.
        /// </summary>
        public virtual DbSet<TreeRoot> TreeRoots { get; set; }
        
        /// <summary>
        /// Узел рабочего дерева.
        /// </summary>
        public virtual DbSet<TreeNode> TreeNodes { get; set; }
        
        /// <summary>
        /// Лист рабочего дерева.
        /// </summary>
        public virtual DbSet<TreeLeave> TreeLeaves { get; set; }
       
        /// <summary>
        /// Атрибут элемента.
        /// </summary>
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
