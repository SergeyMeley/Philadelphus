using Microsoft.Extensions.Caching.Distributed;
using Philadelphus.Infrastructure.Cache.Context;
using Philadelphus.Infrastructure.Cache.RepositoryInterfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Serilog;
using System.Collections.Concurrent;
using System.Text.Json;
using PersistenceAuditInfo = Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties.AuditInfo;

namespace Philadelphus.Infrastructure.Cache.Redis.Implementations
{
    /// <summary>
    /// Кэш содержимого репозитория Philadelphus на базе распределенного кэша
    /// </summary>
    public class DistributedPhiladelphusRepositoryContentCache : IPhiladelphusRepositoryContentCache
    {
        private static readonly JsonSerializerOptions CacheJsonSerializerOptions = new(JsonSerializerDefaults.Web);
        private static readonly TimeSpan DistributedCacheFailurePause = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan CacheRemoveRetryDelay = TimeSpan.FromMilliseconds(50);
        private const int CacheRemoveRetryCount = 3;

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, long> _localCacheVersions = new();
        private DateTime _distributedCacheUnavailableUntilUtc = DateTime.MinValue;

        /// <summary>
        /// Кэш содержимого репозитория Philadelphus на базе распределенного кэша
        /// </summary>
        /// <param name="distributedCache">Распределенный кэш</param>
        /// <param name="logger">Сервис логгирования</param>
        public DistributedPhiladelphusRepositoryContentCache(
            IDistributedCache distributedCache,
            ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(distributedCache);
            ArgumentNullException.ThrowIfNull(logger);

            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <summary>
        /// Прочитать рабочие деревья из кэша Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей рабочих деревьев или null, если кэш пуст или недоступен</returns>
        public IReadOnlyCollection<WorkingTree>? SelectTreesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            return GetCache<WorkingTree>(
                GetTreesCacheKey(dataStorageUuid, uuids),
                cacheReadContext);
        }

        /// <summary>
        /// Записать рабочие деревья в кэш Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей рабочих деревьев</param>
        public void SetTreesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            IEnumerable<WorkingTree> items)
        {
            SetCache(
                GetTreesCacheKey(dataStorageUuid, uuids),
                items,
                CloneForCache);
        }

        /// <summary>
        /// Прочитать корни деревьев из кэша Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей корней или null, если кэш пуст или недоступен</returns>
        public IReadOnlyCollection<TreeRoot>? SelectRootsCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            return GetCache<TreeRoot>(
                GetRootsCacheKey(dataStorageUuid, owningTreesUuids),
                cacheReadContext);
        }

        /// <summary>
        /// Записать корни деревьев в кэш Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей корней</param>
        public void SetRootsCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<TreeRoot> items)
        {
            SetCache(
                GetRootsCacheKey(dataStorageUuid, owningTreesUuids),
                items,
                CloneForCache);
        }

        /// <summary>
        /// Прочитать узлы деревьев из кэша Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей узлов или null, если кэш пуст или недоступен</returns>
        public IReadOnlyCollection<TreeNode>? SelectNodesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            return GetCache<TreeNode>(
                GetNodesCacheKey(dataStorageUuid, owningTreesUuids),
                cacheReadContext);
        }

        /// <summary>
        /// Записать узлы деревьев в кэш Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей узлов</param>
        public void SetNodesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<TreeNode> items)
        {
            SetCache(
                GetNodesCacheKey(dataStorageUuid, owningTreesUuids),
                items,
                CloneForCache);
        }

        /// <summary>
        /// Прочитать листья деревьев из кэша Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей листьев или null, если кэш пуст или недоступен</returns>
        public IReadOnlyCollection<TreeLeave>? SelectLeavesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            return GetCache<TreeLeave>(
                GetLeavesCacheKey(dataStorageUuid, owningTreesUuids),
                cacheReadContext);
        }

        /// <summary>
        /// Записать листья деревьев в кэш Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей листьев</param>
        public void SetLeavesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<TreeLeave> items)
        {
            SetCache(
                GetLeavesCacheKey(dataStorageUuid, owningTreesUuids),
                items,
                CloneForCache);
        }

        /// <summary>
        /// Прочитать атрибуты деревьев из кэша Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей атрибутов или null, если кэш пуст или недоступен</returns>
        public IReadOnlyCollection<ElementAttribute>? SelectAttributesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            return GetCache<ElementAttribute>(
                GetAttributesCacheKey(dataStorageUuid, owningTreesUuids),
                cacheReadContext);
        }

        /// <summary>
        /// Записать атрибуты деревьев в кэш Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <param name="items">Коллекция сущностей атрибутов</param>
        public void SetAttributesCache(
            Guid dataStorageUuid,
            Guid[] owningTreesUuids,
            IEnumerable<ElementAttribute> items)
        {
            SetCache(
                GetAttributesCacheKey(dataStorageUuid, owningTreesUuids),
                items,
                CloneForCache);
        }

        /// <summary>
        /// Удалить кэш рабочих деревьев из Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев</param>
        public void InvalidateTrees(Guid dataStorageUuid, Guid[]? uuids)
        {
            RemoveCache(GetTreesCacheBaseKey(dataStorageUuid, uuids));
        }

        /// <summary>
        /// Удалить кэш содержимого рабочего дерева из Redis
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="treeUuid">Идентификатор рабочего дерева</param>
        public void InvalidateTreeContent(Guid dataStorageUuid, Guid treeUuid)
        {
            var treeUuids = new[] { treeUuid };
            RemoveCache(GetRootsCacheBaseKey(dataStorageUuid, treeUuids));
            RemoveCache(GetNodesCacheBaseKey(dataStorageUuid, treeUuids));
            RemoveCache(GetLeavesCacheBaseKey(dataStorageUuid, treeUuids));
            RemoveCache(GetAttributesCacheBaseKey(dataStorageUuid, treeUuids));
        }

        /// <summary>
        /// Получить коллекцию из кэша
        /// </summary>
        /// <typeparam name="T">Тип сущности инфраструктуры</typeparam>
        /// <param name="cacheKey">Ключ кэша</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <returns>Коллекция сущностей инфраструктуры или null, если кэш пуст или недоступен</returns>
        private IReadOnlyCollection<T>? GetCache<T>(
            string cacheKey,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            if (IsDistributedCacheTemporarilyUnavailable())
            {
                cacheReadContext?.MarkDistributedCacheUnavailable();
                return null;
            }

            if (TryGetCacheString(cacheKey, cacheReadContext, out var cachedJson)
                && string.IsNullOrWhiteSpace(cachedJson) == false)
            {
                try
                {
                    return JsonSerializer.Deserialize<List<T>>(cachedJson, CacheJsonSerializerOptions) ?? new List<T>();
                }
                catch (JsonException ex)
                {
                    _logger.Warning(ex, $"Ошибка десериализации кэша '{cacheKey}'. Данные будут загружены из хранилища.");
                }
            }

            return null;
        }

        /// <summary>
        /// Записать коллекцию в кэш
        /// </summary>
        /// <typeparam name="T">Тип сущности инфраструктуры</typeparam>
        /// <param name="cacheKey">Ключ кэша</param>
        /// <param name="items">Коллекция сущностей инфраструктуры</param>
        /// <param name="clone">Функция подготовки сущности к кэшированию</param>
        private void SetCache<T>(
            string cacheKey,
            IEnumerable<T> items,
            Func<T, T> clone)
        {
            var cacheItems = items
                .Select(clone)
                .ToList();

            if (IsDistributedCacheTemporarilyUnavailable())
            {
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(cacheItems, CacheJsonSerializerOptions);
                _distributedCache.SetString(cacheKey, json, CreateDistributedCacheEntryOptions());
            }
            catch (Exception ex)
            {
                MarkDistributedCacheUnavailable(ex, "обновления", cacheKey);
            }

        }

        /// <summary>
        /// Попробовать прочитать строку из распределенного кэша
        /// </summary>
        /// <param name="cacheKey">Ключ кэша</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных</param>
        /// <param name="cachedJson">Содержимое кэша</param>
        /// <returns>Признак успешного обращения к кэшу</returns>
        private bool TryGetCacheString(
            string cacheKey,
            InfrastructureCacheReadContext? cacheReadContext,
            out string? cachedJson)
        {
            try
            {
                cachedJson = _distributedCache.GetString(cacheKey);
                return true;
            }
            catch (Exception ex)
            {
                cachedJson = null;
                cacheReadContext?.MarkDistributedCacheUnavailable();
                MarkDistributedCacheUnavailable(ex, "чтения", cacheKey);
                return false;
            }
        }

        /// <summary>
        /// Проверить временную недоступность распределенного кэша
        /// </summary>
        /// <returns>Признак временной недоступности распределенного кэша</returns>
        private bool IsDistributedCacheTemporarilyUnavailable()
        {
            return DateTime.UtcNow < _distributedCacheUnavailableUntilUtc;
        }

        /// <summary>
        /// Отметить распределенный кэш временно недоступным
        /// </summary>
        /// <param name="ex">Исключение операции кэша</param>
        /// <param name="operationName">Название операции</param>
        /// <param name="cacheKey">Ключ кэша</param>
        private void MarkDistributedCacheUnavailable(Exception ex, string operationName, string cacheKey)
        {
            _distributedCacheUnavailableUntilUtc = DateTime.UtcNow.Add(DistributedCacheFailurePause);
            _logger.Warning(
                ex,
                $"Ошибка {operationName} кэша '{cacheKey}'. Кэш временно отключен, данные будут загружены из хранилища.");
        }

        /// <summary>
        /// Удалить запись кэша
        /// </summary>
        /// <param name="cacheBaseKey">Базовый ключ кэша</param>
        private void RemoveCache(string cacheBaseKey)
        {
            var staleCacheKey = GetVersionedCacheKey(cacheBaseKey);
            AdvanceLocalCacheVersion(cacheBaseKey);

            for (var attempt = 1; attempt <= CacheRemoveRetryCount; attempt++)
            {
                try
                {
                    _distributedCache.Remove(staleCacheKey);
                    return;
                }
                catch (Exception ex)
                {
                    if (attempt == CacheRemoveRetryCount)
                    {
                        MarkDistributedCacheUnavailable(ex, "удаления", staleCacheKey);
                        return;
                    }

                    Thread.Sleep(CacheRemoveRetryDelay);
                }
            }
        }

        /// <summary>
        /// Получить ключ кэша рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Ключ кэша</returns>
        private string GetTreesCacheKey(Guid dataStorageUuid, Guid[]? uuids)
        {
            return GetVersionedCacheKey(GetTreesCacheBaseKey(dataStorageUuid, uuids));
        }

        /// <summary>
        /// Получить ключ кэша корней рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Ключ кэша</returns>
        private string GetRootsCacheKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetVersionedCacheKey(GetRootsCacheBaseKey(dataStorageUuid, owningTreesUuids));
        }

        /// <summary>
        /// Получить ключ кэша узлов рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Ключ кэша</returns>
        private string GetNodesCacheKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetVersionedCacheKey(GetNodesCacheBaseKey(dataStorageUuid, owningTreesUuids));
        }

        /// <summary>
        /// Получить ключ кэша листьев рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Ключ кэша</returns>
        private string GetLeavesCacheKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetVersionedCacheKey(GetLeavesCacheBaseKey(dataStorageUuid, owningTreesUuids));
        }

        /// <summary>
        /// Получить ключ кэша атрибутов рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Ключ кэша</returns>
        private string GetAttributesCacheKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetVersionedCacheKey(GetAttributesCacheBaseKey(dataStorageUuid, owningTreesUuids));
        }

        /// <summary>
        /// Получить базовый ключ кэша рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Базовый ключ кэша</returns>
        private static string GetTreesCacheBaseKey(Guid dataStorageUuid, Guid[]? uuids)
        {
            return GetCacheBaseKey(dataStorageUuid, "trees", uuids);
        }

        /// <summary>
        /// Получить базовый ключ кэша корней рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Базовый ключ кэша</returns>
        private static string GetRootsCacheBaseKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetCacheBaseKey(dataStorageUuid, "roots", owningTreesUuids);
        }

        /// <summary>
        /// Получить базовый ключ кэша узлов рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Базовый ключ кэша</returns>
        private static string GetNodesCacheBaseKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetCacheBaseKey(dataStorageUuid, "nodes", owningTreesUuids);
        }

        /// <summary>
        /// Получить базовый ключ кэша листьев рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Базовый ключ кэша</returns>
        private static string GetLeavesCacheBaseKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetCacheBaseKey(dataStorageUuid, "leaves", owningTreesUuids);
        }

        /// <summary>
        /// Получить базовый ключ кэша атрибутов рабочих деревьев
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Базовый ключ кэша</returns>
        private static string GetAttributesCacheBaseKey(Guid dataStorageUuid, Guid[]? owningTreesUuids)
        {
            return GetCacheBaseKey(dataStorageUuid, "attributes", owningTreesUuids);
        }

        /// <summary>
        /// Получить ключ кэша с локальной версией
        /// </summary>
        /// <param name="cacheBaseKey">Базовый ключ кэша</param>
        /// <returns>Версионированный ключ кэша</returns>
        private string GetVersionedCacheKey(string cacheBaseKey)
        {
            var version = _localCacheVersions.GetOrAdd(cacheBaseKey, 0);
            return $"{cacheBaseKey}:v{version}";
        }

        /// <summary>
        /// Увеличить локальную версию ключа кэша
        /// </summary>
        /// <param name="cacheBaseKey">Базовый ключ кэша</param>
        private void AdvanceLocalCacheVersion(string cacheBaseKey)
        {
            _localCacheVersions.AddOrUpdate(cacheBaseKey, 1, (_, version) => version + 1);
        }

        /// <summary>
        /// Получить ключ кэша
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных</param>
        /// <param name="entityGroup">Группа сущностей</param>
        /// <param name="uuids">Идентификаторы сущностей или владельцев</param>
        /// <returns>Ключ кэша</returns>
        private static string GetCacheBaseKey(Guid dataStorageUuid, string entityGroup, Guid[]? uuids)
        {
            var normalizedUuids = uuids == null || uuids.Length == 0
                ? "all"
                : string.Join("-", uuids.OrderBy(x => x).Select(x => x.ToString("N")));

            return $"Philadelphus:CoreDomain:v1:storage:{dataStorageUuid:N}:{entityGroup}:{normalizedUuids}";
        }

        /// <summary>
        /// Создать параметры записи распределенного кэша
        /// </summary>
        /// <returns>Параметры записи кэша</returns>
        private static DistributedCacheEntryOptions CreateDistributedCacheEntryOptions()
        {
            return new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            };
        }

        /// <summary>
        /// Подготовить сущность рабочего дерева к кэшированию
        /// </summary>
        /// <param name="item">Сущность рабочего дерева</param>
        /// <returns>Сущность без навигационных свойств</returns>
        private static WorkingTree CloneForCache(WorkingTree item)
        {
            return new WorkingTree
            {
                Uuid = item.Uuid,
                Name = item.Name,
                Description = item.Description,
                Sequence = item.Sequence,
                Alias = item.Alias,
                CustomCode = item.CustomCode,
                IsHidden = item.IsHidden,
                AuditInfo = CloneAuditInfo(item.AuditInfo),
                OwnDataStorageUuid = item.OwnDataStorageUuid
            };
        }

        /// <summary>
        /// Подготовить сущность корня к кэшированию
        /// </summary>
        /// <param name="item">Сущность корня</param>
        /// <returns>Сущность без навигационных свойств</returns>
        private static TreeRoot CloneForCache(TreeRoot item)
        {
            return new TreeRoot
            {
                Uuid = item.Uuid,
                Name = item.Name,
                Description = item.Description,
                Sequence = item.Sequence,
                Alias = item.Alias,
                CustomCode = item.CustomCode,
                IsHidden = item.IsHidden,
                AuditInfo = CloneAuditInfo(item.AuditInfo),
                OwningWorkingTreeUuid = item.OwningWorkingTreeUuid
            };
        }

        /// <summary>
        /// Подготовить сущность узла к кэшированию
        /// </summary>
        /// <param name="item">Сущность узла</param>
        /// <returns>Сущность без навигационных свойств</returns>
        private static TreeNode CloneForCache(TreeNode item)
        {
            return new TreeNode
            {
                Uuid = item.Uuid,
                Name = item.Name,
                Description = item.Description,
                Sequence = item.Sequence,
                Alias = item.Alias,
                CustomCode = item.CustomCode,
                IsHidden = item.IsHidden,
                AuditInfo = CloneAuditInfo(item.AuditInfo),
                OwningWorkingTreeUuid = item.OwningWorkingTreeUuid,
                ParentTreeRootUuid = item.ParentTreeRootUuid,
                ParentTreeNodeUuid = item.ParentTreeNodeUuid,
                SystemBaseTypeId = item.SystemBaseTypeId
            };
        }

        /// <summary>
        /// Подготовить сущность листа к кэшированию
        /// </summary>
        /// <param name="item">Сущность листа</param>
        /// <returns>Сущность без навигационных свойств</returns>
        private static TreeLeave CloneForCache(TreeLeave item)
        {
            return new TreeLeave
            {
                Uuid = item.Uuid,
                Name = item.Name,
                Description = item.Description,
                Sequence = item.Sequence,
                Alias = item.Alias,
                CustomCode = item.CustomCode,
                IsHidden = item.IsHidden,
                AuditInfo = CloneAuditInfo(item.AuditInfo),
                OwningWorkingTreeUuid = item.OwningWorkingTreeUuid,
                ParentTreeNodeUuid = item.ParentTreeNodeUuid,
                SystemBaseTypeId = item.SystemBaseTypeId
            };
        }

        /// <summary>
        /// Подготовить сущность атрибута к кэшированию
        /// </summary>
        /// <param name="item">Сущность атрибута</param>
        /// <returns>Сущность без навигационных свойств</returns>
        private static ElementAttribute CloneForCache(ElementAttribute item)
        {
            return new ElementAttribute
            {
                Uuid = item.Uuid,
                Name = item.Name,
                Description = item.Description,
                Sequence = item.Sequence,
                Alias = item.Alias,
                CustomCode = item.CustomCode,
                IsHidden = item.IsHidden,
                AuditInfo = CloneAuditInfo(item.AuditInfo),
                OwningWorkingTreeUuid = item.OwningWorkingTreeUuid,
                DeclaringUuid = item.DeclaringUuid,
                OwnerUuid = item.OwnerUuid,
                DeclaringOwnerUuid = item.DeclaringOwnerUuid,
                ValueTypeUuid = item.ValueTypeUuid,
                ValueUuid = item.ValueUuid,
                IsCollectionValue = item.IsCollectionValue,
                ValuesUuids = item.ValuesUuids,
                VisibilityId = item.VisibilityId,
                OverrideId = item.OverrideId
            };
        }

        /// <summary>
        /// Клонировать информацию аудита
        /// </summary>
        /// <param name="auditInfo">Информация аудита</param>
        /// <returns>Копия информации аудита</returns>
        private static PersistenceAuditInfo CloneAuditInfo(PersistenceAuditInfo? auditInfo)
        {
            if (auditInfo == null)
            {
                return new PersistenceAuditInfo();
            }

            return new PersistenceAuditInfo
            {
                CreatedAt = auditInfo.CreatedAt,
                CreatedBy = auditInfo.CreatedBy,
                UpdatedAt = auditInfo.UpdatedAt,
                UpdatedBy = auditInfo.UpdatedBy,
                IsDeleted = auditInfo.IsDeleted,
                DeletedAt = auditInfo.DeletedAt,
                DeletedBy = auditInfo.DeletedBy
            };
        }
    }
}
