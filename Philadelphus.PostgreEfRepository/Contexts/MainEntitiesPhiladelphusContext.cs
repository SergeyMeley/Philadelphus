using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.PostgreEfRepository.Configurations;
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

        public virtual DbSet<TreeRoot> TreeRoots { get; set; }
        public virtual DbSet<TreeNode> TreeNodes { get; set; }
        public virtual DbSet<TreeLeave> TreeLeaves { get; set; }
        public virtual DbSet<ElementAttribute> ElementAttributes { get; set; }

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
            modelBuilder.Ignore<TreeRootMemberBase>();
            modelBuilder.ApplyConfiguration(new TreeRootConfiguration());
            modelBuilder.ApplyConfiguration(new TreeNodeConfiguration());
            modelBuilder.ApplyConfiguration(new TreeLeaveConfiguration());
            modelBuilder.ApplyConfiguration(new ElementAttributeConfiguration());
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
