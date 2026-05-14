using Microsoft.EntityFrameworkCore;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    /// <summary>
    /// Репозиторий доступа к данным участника кустарника.
    /// </summary>
    public class PostgreEfShrubMembersInfrastructureRepository : PostgreEfInfrastructureRepositoryBase<PostgreEfShrubMembersContext>, IShrubMembersInfrastructureRepository
    {
        /// <summary>
        /// Группа инфраструктурных сущностей.
        /// </summary>
        public override InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.ShrubMembers; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PostgreEfShrubMembersInfrastructureRepository" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="connectionString">Строка подключения.</param>
        public PostgreEfShrubMembersInfrastructureRepository(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }

        protected override PostgreEfShrubMembersContext GetNewContext() => new PostgreEfShrubMembersContext(_connectionString);

        protected override DbSet<TEntity> GetDbSet<TEntity>(PostgreEfShrubMembersContext context) where TEntity : class
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

        #region [ Select ]

        /// <summary>
        /// Выполняет операцию SelectTreeAggregates.
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы.</param>
        /// <returns>Коллекция полученных данных.</returns>
        public IEnumerable<WorkingTree> SelectTreeAggregates(Guid[]? uuids = null)
            => SelectWorkingTreeAggregates(uuids);

        #endregion

        #region [ Insert ]

        /// <summary>
        /// Выполняет операцию InsertTrees.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertTrees(IEnumerable<WorkingTree> items)
            => Insert(items);

        /// <summary>
        /// Выполняет операцию InsertRoots.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertRoots(IEnumerable<TreeRoot> items)
            => Insert(items);

        /// <summary>
        /// Выполняет операцию InsertNodes.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertNodes(IEnumerable<TreeNode> items)
            => Insert(items);

        /// <summary>
        /// Выполняет операцию InsertLeaves.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertLeaves(IEnumerable<TreeLeave> items)
            => Insert(items);

        /// <summary>
        /// Выполняет операцию InsertAttributes.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertAttributes(IEnumerable<ElementAttribute> items)
            => Insert(items);

        #endregion

        #region [ Update ]

        /// <summary>
        /// Обновляет данные UpdateTrees.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateTrees(IEnumerable<WorkingTree> items)
            => Update(items);

        /// <summary>
        /// Обновляет данные UpdateRoots.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateRoots(IEnumerable<TreeRoot> items)
            => Update(items);

        /// <summary>
        /// Обновляет данные UpdateNodes.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateNodes(IEnumerable<TreeNode> items)
            => Update(items);

        /// <summary>
        /// Обновляет данные UpdateLeaves.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateLeaves(IEnumerable<TreeLeave> items)
            => Update(items);

        /// <summary>
        /// Обновляет данные UpdateAttributes.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateAttributes(IEnumerable<ElementAttribute> items)
            => Update(items);

        #endregion

        #region [ Delete ]

        /// <summary>
        /// Выполняет операцию SoftDeleteTrees.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteTrees(IEnumerable<WorkingTree> items)
            => SoftDelete(items);

        /// <summary>
        /// Выполняет операцию SoftDeleteRoots.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteRoots(IEnumerable<TreeRoot> items)
            => SoftDelete(items);

        /// <summary>
        /// Выполняет операцию SoftDeleteNodes.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteNodes(IEnumerable<TreeNode> items)
            => SoftDelete(items);

        /// <summary>
        /// Выполняет операцию SoftDeleteLeaves.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteLeaves(IEnumerable<TreeLeave> items)
            => SoftDelete(items);

        /// <summary>
        /// Выполняет операцию SoftDeleteAttributes.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteAttributes(IEnumerable<ElementAttribute> items)
            => SoftDelete(items);

        #endregion
    }
}
