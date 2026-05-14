using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.EF.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts
{
    /// <summary>
    /// Контекст данных для участников кустарника.
    /// </summary>
    public partial class PostgreEfShrubMembersContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfShrubMembersContext" />.
        /// </summary>
        public PostgreEfShrubMembersContext()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfShrubMembersContext" />.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        public PostgreEfShrubMembersContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfShrubMembersContext" />.
        /// </summary>
        /// <param name="options">Параметры конфигурации приложения.</param>
        public PostgreEfShrubMembersContext(DbContextOptions<PostgreEfShrubMembersContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Рабочие деревья.
        /// </summary>
        public virtual DbSet<WorkingTree> WorkingTrees { get; set; }
        
        /// <summary>
        /// Корни рабочих деревьев.
        /// </summary>
        public virtual DbSet<TreeRoot> TreeRoots { get; set; }
        
        /// <summary>
        /// Узлы рабочих деревьев.
        /// </summary>
        public virtual DbSet<TreeNode> TreeNodes { get; set; }
        
        /// <summary>
        /// Листы рабочих деревьев.
        /// </summary>
        public virtual DbSet<TreeLeave> TreeLeaves { get; set; }

        /// <summary>
        /// Атрибут элементов рабочих деревьев.
        /// </summary>
        public virtual DbSet<ElementAttribute> ElementAttributes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseNpgsql(_connectionString)
                    .UseLazyLoadingProxies();
                    //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    //.LogTo(Log.Information, LogLevel.Information)
                    //.EnableSensitiveDataLogging()
                    //.EnableDetailedErrors();
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
