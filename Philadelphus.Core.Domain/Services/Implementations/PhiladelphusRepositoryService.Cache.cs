using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Infrastructure.Cache.Context;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Кэшируемая часть сервиса для работы с репозиторием и его элементами
    /// </summary>
    public partial class PhiladelphusRepositoryService
    {
        /// <summary>
        /// Быстро получить содержимое репозитория из кэша или хранилища
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Репозиторий с содержимым</returns>
        private PhiladelphusRepositoryModel GetShrubContentFast(PhiladelphusRepositoryModel repository, InfrastructureCacheReadContext cacheReadContext)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repository.ContentShrub);
            ArgumentNullException.ThrowIfNull(cacheReadContext);

            repository.ContentShrub.ContentWorkingTrees.Clear();

            foreach (var dataStorage in repository.DataStorages ?? Enumerable.Empty<IDataStorageModel>())
            {
                if (dataStorage.IsAvailable)
                {
                    var treesUuids = repository.ContentShrub.ContentWorkingTreesUuids?.ToArray();
                    var dbTrees = SelectTreesFast(dataStorage, treesUuids, cacheReadContext);
                    var trees = _mapper.MapWorkingTrees(dbTrees, repository.DataStorages, repository.ContentShrub, _notificationService, new EmptyPropertiesPolicy<WorkingTreeModel>());

                    foreach (var tree in trees?.Where(x => x.OwningRepository.Uuid == repository.Uuid))
                    {
                        tree.UnavailableNames.Add(tree.Name);

                        repository.ContentShrub.ContentWorkingTrees.Add(tree);

                        SetModelState(tree, State.SavedOrLoaded);
                    }

                    foreach (var tree in repository.ContentShrub.ContentWorkingTrees)
                    {
                        GetWorkingTreeContentFast(tree, cacheReadContext);
                    }
                }
            }

            InitSystemWorkingTree(repository.ContentShrub);

            SetModelState(repository, State.SavedOrLoaded);

            return repository;
        }

        /// <summary>
        /// Быстро получить элементы рабочего дерева из кэша или хранилища
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Дерево с содержимым</returns>
        private WorkingTreeModel GetWorkingTreeContentFast(WorkingTreeModel tree, InfrastructureCacheReadContext cacheReadContext)
        {
            ArgumentNullException.ThrowIfNull(cacheReadContext);

            tree.ContentRoot = null!;
            GetWorkingTreeContentWithoutAttributesFast(tree, cacheReadContext);

            var allShrubNodes = tree.OwningShrub.ContentWorkingTrees.SelectMany(x => x.GetAllNodesRecursive() ?? new List<TreeNodeModel>()).ToList();
            var allShrubLeaves = tree.OwningShrub.ContentWorkingTrees.SelectMany(x => x.GetAllLeavesRecursive() ?? new List<TreeLeaveModel>()).ToList();

            var owners = new List<IAttributeOwnerModel>();
            owners.Add(tree);
            if (tree.ContentRoot != null)
            {
                owners.Add(tree.ContentRoot);
            }

            owners.AddRange(tree.GetAllNodesRecursive() ?? Enumerable.Empty<TreeNodeModel>());
            owners.AddRange(tree.GetAllLeavesRecursive() ?? Enumerable.Empty<TreeLeaveModel>());

            var dbAttributes = SelectAttributesFast(tree.OwnDataStorage, new[] { tree.Uuid }, cacheReadContext);
            var allAttributes = _mapper.MapAttributes(dbAttributes, owners, allShrubNodes, allShrubLeaves, tree, _notificationService, AttributePolicyBuilder.CreateDefault(_notificationService));

            foreach (var item in owners)
            {
                DistributeAttributes(item, allAttributes, allShrubNodes, allShrubLeaves);
            }

            return tree;
        }

        /// <summary>
        /// Быстро получить элементы рабочего дерева без атрибутов из кэша или хранилища
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Дерево с содержимым без атрибутов</returns>
        private WorkingTreeModel GetWorkingTreeContentWithoutAttributesFast(
            WorkingTreeModel tree,
            InfrastructureCacheReadContext cacheReadContext)
        {
            ArgumentNullException.ThrowIfNull(tree);
            ArgumentNullException.ThrowIfNull(cacheReadContext);

            tree.ContentRoot = null!;

            if (tree.DataStorage.IsAvailable)
            {
                var treeUuids = new[] { tree.Uuid };
                var dbRoots = SelectRootsFast(tree.DataStorage, treeUuids, cacheReadContext);
                var dbNodes = SelectNodesFast(tree.DataStorage, treeUuids, cacheReadContext);
                var dbLeaves = SelectLeavesFast(tree.DataStorage, treeUuids, cacheReadContext);

                ApplyWorkingTreeContentWithoutAttributes(tree, dbRoots, dbNodes, dbLeaves);
            }

            return tree;
        }

        /// <summary>
        /// Удалить содержимое репозитория из кэша
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        private void InvalidateCachedContent(PhiladelphusRepositoryModel repository)
        {
            foreach (var dataStorage in repository.DataStorages ?? Enumerable.Empty<IDataStorageModel>())
            {
                _contentCache.InvalidateTrees(dataStorage.Uuid, repository.ContentShrub?.ContentWorkingTreesUuids?.ToArray());
            }

            foreach (var tree in repository.ContentShrub?.ContentWorkingTrees ?? Enumerable.Empty<WorkingTreeModel>())
            {
                InvalidateCachedContent(tree);
            }
        }

        /// <summary>
        /// Удалить содержимое рабочего дерева из кэша
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        private void InvalidateCachedContent(WorkingTreeModel tree)
        {
            _contentCache.InvalidateTreeContent(tree.DataStorage.Uuid, tree.Uuid);

            if (tree.OwningRepository != null)
            {
                foreach (var dataStorage in tree.OwningRepository.DataStorages ?? Enumerable.Empty<IDataStorageModel>())
                {
                    _contentCache.InvalidateTrees(dataStorage.Uuid, tree.OwningShrub.ContentWorkingTreesUuids?.ToArray());
                }
            }
        }

        /// <summary>
        /// Удалить содержимое рабочих деревьев из кэша
        /// </summary>
        /// <param name="workingTrees">Коллекция рабочих деревьев</param>
        private void InvalidateCachedContent(IEnumerable<WorkingTreeModel> workingTrees)
        {
            foreach (var tree in workingTrees
                .Where(x => x != null)
                .GroupBy(x => x.Uuid)
                .Select(x => x.First()))
            {
                InvalidateCachedContent(tree);
            }
        }

        /// <summary>
        /// Удалить из кэша содержимое рабочих деревьев, которым принадлежат элементы
        /// </summary>
        /// <param name="workingTreeMembers">Коллекция элементов рабочих деревьев</param>
        private void InvalidateCachedContent(IEnumerable<IWorkingTreeMemberModel> workingTreeMembers)
        {
            foreach (var tree in workingTreeMembers
                .Where(x => x?.OwningWorkingTree != null)
                .Select(x => x.OwningWorkingTree)
                .GroupBy(x => x.Uuid)
                .Select(x => x.First()))
            {
                InvalidateCachedContent(tree);
            }
        }

        /// <summary>
        /// Удалить из кэша содержимое рабочих деревьев, которым принадлежат атрибуты
        /// </summary>
        /// <param name="elementAttributes">Коллекция атрибутов</param>
        private void InvalidateCachedContent(IEnumerable<ElementAttributeModel> elementAttributes)
        {
            foreach (var tree in elementAttributes
                .Where(x => x?.OwningWorkingTree != null)
                .Select(x => x.OwningWorkingTree)
                .GroupBy(x => x.Uuid)
                .Select(x => x.First()))
            {
                InvalidateCachedContent(tree);
            }
        }

        /// <summary>
        /// Обновить кэш рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Сущности рабочих деревьев</param>
        private void RefreshTreesCache(Guid dataStorageUuid, Guid[]? uuids, IReadOnlyCollection<WorkingTree> items)
        {
            _contentCache.SetTreesCache(dataStorageUuid, uuids, items);
        }

        /// <summary>
        /// Обновить кэш корней рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Сущности корней</param>
        private void RefreshRootsCache(Guid dataStorageUuid, Guid[] owningTreesUuids, IReadOnlyCollection<TreeRoot> items)
        {
            _contentCache.SetRootsCache(dataStorageUuid, owningTreesUuids, items);
        }

        /// <summary>
        /// Обновить кэш узлов рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Сущности узлов</param>
        private void RefreshNodesCache(Guid dataStorageUuid, Guid[] owningTreesUuids, IReadOnlyCollection<TreeNode> items)
        {
            _contentCache.SetNodesCache(dataStorageUuid, owningTreesUuids, items);
        }

        /// <summary>
        /// Обновить кэш листьев рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Сущности листьев</param>
        private void RefreshLeavesCache(Guid dataStorageUuid, Guid[] owningTreesUuids, IReadOnlyCollection<TreeLeave> items)
        {
            _contentCache.SetLeavesCache(dataStorageUuid, owningTreesUuids, items);
        }

        /// <summary>
        /// Обновить кэш атрибутов рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Сущности атрибутов</param>
        private void RefreshAttributesCache(Guid dataStorageUuid, Guid[] owningTreesUuids, IReadOnlyCollection<ElementAttribute> items)
        {
            _contentCache.SetAttributesCache(dataStorageUuid, owningTreesUuids, items);
        }

        /// <summary>
        /// Прочитать рабочие деревья из кэша или хранилища
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="uuids">Идентификаторы деревьев</param>
        /// <returns>Коллекция сущностей рабочих деревьев</returns>
        private IReadOnlyCollection<WorkingTree> SelectTreesFast(
            IDataStorageModel dataStorage,
            Guid[]? uuids,
            InfrastructureCacheReadContext cacheReadContext)
        {
            cacheReadContext.MarkStorageRead();
            return SelectTreesForce(dataStorage, uuids);
        }

        /// <summary>
        /// Прочитать корни деревьев из кэша или хранилища
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей корней</returns>
        private IReadOnlyCollection<TreeRoot> SelectRootsFast(
            IDataStorageModel dataStorage,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            var cachedItems = _contentCache.SelectRootsCache(
                dataStorage.Uuid,
                owningTreesUuids,
                cacheReadContext);

            if (cachedItems != null)
            {
                if (IsRootsSetValid(cachedItems, owningTreesUuids))
                {
                    return cachedItems;
                }

                foreach (var treeUuid in owningTreesUuids.Distinct())
                {
                    _contentCache.InvalidateTreeContent(dataStorage.Uuid, treeUuid);
                }
            }

            cacheReadContext?.MarkStorageRead();
            var items = SelectRootsForce(dataStorage, owningTreesUuids);
            if (IsRootsSetValid(items, owningTreesUuids))
            {
                RefreshRootsCache(dataStorage.Uuid, owningTreesUuids, items);
            }

            return items;
        }

        /// <summary>
        /// Проверить инвариант корней рабочих деревьев
        /// </summary>
        /// <param name="items">Коллекция корней</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Признак корректного набора корней</returns>
        private static bool IsRootsSetValid(IReadOnlyCollection<TreeRoot> items, Guid[] owningTreesUuids)
        {
            var expectedTreeUuids = owningTreesUuids.Distinct().ToArray();

            return items.Count == expectedTreeUuids.Length
                && expectedTreeUuids.All(treeUuid => items.Count(root => root.OwningWorkingTreeUuid == treeUuid) == 1);
        }

        /// <summary>
        /// Прочитать узлы деревьев из кэша или хранилища
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей узлов</returns>
        private IReadOnlyCollection<TreeNode> SelectNodesFast(
            IDataStorageModel dataStorage,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            var cachedItems = _contentCache.SelectNodesCache(
                dataStorage.Uuid,
                owningTreesUuids,
                cacheReadContext);

            if (cachedItems != null)
            {
                return cachedItems;
            }

            cacheReadContext?.MarkStorageRead();
            var items = SelectNodesForce(dataStorage, owningTreesUuids);
            RefreshNodesCache(dataStorage.Uuid, owningTreesUuids, items);

            return items;
        }

        /// <summary>
        /// Прочитать листья деревьев из кэша или хранилища
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей листьев</returns>
        private IReadOnlyCollection<TreeLeave> SelectLeavesFast(
            IDataStorageModel dataStorage,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            var cachedItems = _contentCache.SelectLeavesCache(
                dataStorage.Uuid,
                owningTreesUuids,
                cacheReadContext);

            if (cachedItems != null)
            {
                return cachedItems;
            }

            cacheReadContext?.MarkStorageRead();
            var items = SelectLeavesForce(dataStorage, owningTreesUuids);
            RefreshLeavesCache(dataStorage.Uuid, owningTreesUuids, items);

            return items;
        }

        /// <summary>
        /// Прочитать атрибуты деревьев из кэша или хранилища
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей атрибутов</returns>
        private IReadOnlyCollection<ElementAttribute> SelectAttributesFast(
            IDataStorageModel dataStorage,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext cacheReadContext)
        {
            var cachedItems = _contentCache.SelectAttributesCache(
                dataStorage.Uuid,
                owningTreesUuids,
                cacheReadContext);

            if (cachedItems != null)
            {
                return cachedItems;
            }

            cacheReadContext.MarkStorageRead();
            var items = SelectAttributesForce(dataStorage, owningTreesUuids);
            RefreshAttributesCache(dataStorage.Uuid, owningTreesUuids, items);

            return items;
        }
    }
}
