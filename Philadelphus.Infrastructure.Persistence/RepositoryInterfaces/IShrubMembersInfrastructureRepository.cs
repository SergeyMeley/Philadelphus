using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    /// <summary>
    /// Задает контракт для работы с репозиторем БД участиков кустарника.
    /// </summary>
    public interface IShrubMembersInfrastructureRepository : IInfrastructureRepository
    {
        # region [ Select ]

        /// <summary>
        /// Получить агрегаты рабочих деревьев.
        /// </summary>
        /// <param name="uuids">Уникальные идентификаторы.</param>
        /// <returns>Коллекция полученных данных.</returns>
        public IEnumerable<WorkingTree> SelectTreeAggregates(Guid[]? uuids = null);

        #endregion

        #region [ Insert ]

        /// <summary>
        /// Добавить рабочие деревья.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertTrees(IEnumerable<WorkingTree> items);
        
        /// <summary>
        /// Добавить корни.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertRoots(IEnumerable<TreeRoot> items);
        
        /// <summary>
        /// Добавить узлы.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertNodes(IEnumerable<TreeNode> items);
       
        /// <summary>
        /// Добавить листы.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertLeaves(IEnumerable<TreeLeave> items);
        
        /// <summary>
        /// Добавить атрибуты.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long InsertAttributes(IEnumerable<ElementAttribute> items);

        #endregion

        #region [ Update ]

        /// <summary>
        /// Обновить рабочие деревья.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateTrees(IEnumerable<WorkingTree> items);
       
        /// <summary>
        /// Обновить корни.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateRoots(IEnumerable<TreeRoot> items);
        
        /// <summary>
        /// Обновить узлы.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateNodes(IEnumerable<TreeNode> items);
       
        /// <summary>
        /// Обновить листы.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateLeaves(IEnumerable<TreeLeave> items);
        
        /// <summary>
        /// Обновить атрибуты.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long UpdateAttributes(IEnumerable<ElementAttribute> items);

        #endregion

        #region [ Delete ]

        /// <summary>
        /// Удалить мягко рабочие деревья.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteTrees(IEnumerable<WorkingTree> items);

        /// <summary>
        /// Удалить мягко корни.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteRoots(IEnumerable<TreeRoot> items);

        /// <summary>
        /// Удалить мягко узлы.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteNodes(IEnumerable<TreeNode> items);

        /// <summary>
        /// Удалить мягко листы.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteLeaves(IEnumerable<TreeLeave> items);

        /// <summary>
        /// Удалить мягко атрибуты.
        /// </summary>
        /// <param name="items">Коллекция элементов.</param>
        /// <returns>Результат выполнения операции.</returns>
        public long SoftDeleteAttributes(IEnumerable<ElementAttribute> items);

        #endregion
    }
} 
