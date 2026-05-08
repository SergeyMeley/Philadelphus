using Philadelphus.Infrastructure.Cache.Context;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;

namespace Philadelphus.Infrastructure.Cache.RepositoryInterfaces
{
    /// <summary>
    /// Кэш содержимого репозитория Philadelphus.
    /// </summary>
    public interface IPhiladelphusRepositoryContentCache
    {
        /// <summary>
        /// Прочитать агрегаты рабочих деревьев из кэша.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных.</param>
        /// <returns>Коллекция агрегатов рабочих деревьев или null, если кэш пуст или недоступен.</returns>
        IReadOnlyCollection<WorkingTree>? SelectTreeAggregatesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            InfrastructureCacheReadContext? cacheReadContext);

        /// <summary>
        /// Записать агрегаты рабочих деревьев в кэш.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        /// <param name="items">Коллекция агрегатов рабочих деревьев.</param>
        void SetTreeAggregatesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            IEnumerable<WorkingTree> items);

        /// <summary>
        /// Удалить кэш рабочих деревьев.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        void InvalidateTrees(Guid dataStorageUuid, Guid[]? uuids);

        /// <summary>
        /// Удалить кэш содержимого рабочего дерева.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="treeUuid">Идентификатор рабочего дерева.</param>
        void InvalidateTreeContent(Guid dataStorageUuid, Guid treeUuid);
    }
}
