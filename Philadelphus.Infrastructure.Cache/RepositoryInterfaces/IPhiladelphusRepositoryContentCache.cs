using Philadelphus.Infrastructure.Cache.Context;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Cache.RepositoryInterfaces
{
    /// <summary>
    /// Кэш содержимого репозитория Philadelphus
    /// </summary>
    public interface IPhiladelphusRepositoryContentCache
    {
        /// <summary>
        /// Прочитать рабочие деревья из кэша
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей рабочих деревьев или null, если кэш пуст или недоступен</returns>
        IReadOnlyCollection<WorkingTree>? SelectTreesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            InfrastructureCacheReadContext? cacheReadContext);

        /// <summary>
        /// Записать рабочие деревья в кэш
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы деревьев</param>
        /// <param name="items">Коллекция сущностей рабочих деревьев</param>
        void SetTreesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            IEnumerable<WorkingTree> items);

        /// <summary>
        /// Прочитать корни деревьев из кэша
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей корней или null, если кэш пуст или недоступен</returns>
        IReadOnlyCollection<TreeRoot>? SelectRootsCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext);

        /// <summary>
        /// Записать корни деревьев в кэш
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей корней</param>
        void SetRootsCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<TreeRoot> items);

        /// <summary>
        /// Прочитать узлы деревьев из кэша
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей узлов или null, если кэш пуст или недоступен</returns>
        IReadOnlyCollection<TreeNode>? SelectNodesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext);

        /// <summary>
        /// Записать узлы деревьев в кэш
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей узлов</param>
        void SetNodesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<TreeNode> items);

        /// <summary>
        /// Прочитать листья деревьев из кэша
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей листьев или null, если кэш пуст или недоступен</returns>
        IReadOnlyCollection<TreeLeave>? SelectLeavesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext);

        /// <summary>
        /// Записать листья деревьев в кэш
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей листьев</param>
        void SetLeavesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<TreeLeave> items);

        /// <summary>
        /// Прочитать атрибуты деревьев из кэша
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей атрибутов или null, если кэш пуст или недоступен</returns>
        IReadOnlyCollection<ElementAttribute>? SelectAttributesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext);

        /// <summary>
        /// Записать атрибуты деревьев в кэш
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей атрибутов</param>
        void SetAttributesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<ElementAttribute> items);

        /// <summary>
        /// Удалить кэш рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев</param>
        void InvalidateTrees(Guid dataStorageUuid, Guid[]? uuids);

        /// <summary>
        /// Удалить кэш содержимого рабочего дерева
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="treeUuid">Идентификатор рабочего дерева</param>
        void InvalidateTreeContent(Guid dataStorageUuid, Guid treeUuid);
    }
}
