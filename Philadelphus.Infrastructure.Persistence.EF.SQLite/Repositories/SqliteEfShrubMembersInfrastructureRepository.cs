using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.SQLite.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Repositories
{
    public class SqliteEfShrubMembersInfrastructureRepository : SqliteEfInfrastructureRepositoryBase<SqliteEfShrubMembersContext>, IShrubMembersInfrastructureRepository
    {
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.ShrubMembers; }

        public SqliteEfShrubMembersInfrastructureRepository(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }

        protected override SqliteEfShrubMembersContext GetNewContext() => new SqliteEfShrubMembersContext(_connectionString);

        protected override DbSet<TEntity> GetDbSet<TEntity>(SqliteEfShrubMembersContext context) where TEntity : class
        {
            return typeof(TEntity).Name switch
            {
                nameof(WorkingTree) => context.WorkingTrees as DbSet<TEntity>,
                nameof(TreeRoot) => context.TreeRoots as DbSet<TEntity>,
                nameof(TreeNode) => context.TreeNodes as DbSet<TEntity>,
                nameof(TreeLeave) => context.TreeLeaves as DbSet<TEntity>,
                nameof(ElementAttribute) => context.ElementAttributes as DbSet<TEntity>,
                _ => throw new NotSupportedException($"Тип {typeof(TEntity).Name} не поддерживается.")
            };
        }

        protected override void SetNavigationProperties<TEntity, TNav>(TEntity item, Dictionary<Guid, TNav> navigationEntities)
        {
            if (item is WorkingTreeMemberBase workingTreeMember)
            {
                if (navigationEntities.TryGetValue(workingTreeMember.OwningWorkingTreeUuid, out var owningTree))
                {
                    workingTreeMember.OwningWorkingTree = owningTree as WorkingTree
                        ?? throw new ArgumentNullException();
                }
                else
                {
                    throw new InvalidOperationException($"Родительский WorkingTree с Uuid='{workingTreeMember.OwningWorkingTreeUuid}' не найден в БД");
                }
            }
        }

        #region [ Select ]

        public IEnumerable<WorkingTree> SelectTrees(Guid[] uuids = null)
                => Select<WorkingTree>(ownUuids: uuids);

        public IEnumerable<TreeRoot> SelectRoots(Guid[] owningTreesUuids)
            => Select<TreeRoot>(owningTreesUuids: owningTreesUuids);

        public IEnumerable<TreeNode> SelectNodes(Guid[] owningTreesUuids)
            => Select<TreeNode>(owningTreesUuids: owningTreesUuids);

        public IEnumerable<TreeLeave> SelectLeaves(Guid[] owningTreesUuids)
            => Select<TreeLeave>(owningTreesUuids: owningTreesUuids);

        public IEnumerable<ElementAttribute> SelectAttributes(Guid[] owningTreesUuids)
            => Select<ElementAttribute>(owningTreesUuids: owningTreesUuids);

        #endregion

        #region [ Insert ]

        public long InsertTrees(IEnumerable<WorkingTree> items)
            => Insert(items);

        public long InsertRoots(IEnumerable<TreeRoot> items)
            => Insert(items);

        public long InsertNodes(IEnumerable<TreeNode> items)
            => Insert(items);

        public long InsertLeaves(IEnumerable<TreeLeave> items)
            => Insert(items);

        public long InsertAttributes(IEnumerable<ElementAttribute> items)
            => Insert(items);

        #endregion

        #region [ Update ]

        public long UpdateTrees(IEnumerable<WorkingTree> items)
            => Update(items);

        public long UpdateRoots(IEnumerable<TreeRoot> items)
            => Update(items);

        public long UpdateNodes(IEnumerable<TreeNode> items)
            => Update(items);

        public long UpdateLeaves(IEnumerable<TreeLeave> items)
            => Update(items);

        public long UpdateAttributes(IEnumerable<ElementAttribute> items)
            => Update(items);

        #endregion

        #region [ Delete ]

        public long SoftDeleteTrees(IEnumerable<WorkingTree> items)
            => SoftDelete(items);

        public long SoftDeleteRoots(IEnumerable<TreeRoot> items)
            => SoftDelete(items);

        public long SoftDeleteNodes(IEnumerable<TreeNode> items)
            => SoftDelete(items);

        public long SoftDeleteLeaves(IEnumerable<TreeLeave> items)
            => SoftDelete(items);

        public long SoftDeleteAttributes(IEnumerable<ElementAttribute> items)
            => SoftDelete(items);

        #endregion
    }
}
