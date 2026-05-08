using Microsoft.Extensions.Caching.Distributed;
using Philadelphus.Infrastructure.Cache.Context;
using Philadelphus.Infrastructure.Cache.RepositoryInterfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Serilog;
using System.Text.Json;
using PersistenceAuditInfo = Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties.AuditInfo;

namespace Philadelphus.Infrastructure.Cache.Redis.Implementations
{
    /// <summary>
    /// Кэш содержимого репозитория Philadelphus на базе распределенного кэша.
    /// </summary>
    public class DistributedPhiladelphusRepositoryContentCache : IPhiladelphusRepositoryContentCache
    {
        private static readonly JsonSerializerOptions CacheJsonSerializerOptions = new(JsonSerializerDefaults.Web);
        private static readonly TimeSpan DistributedCacheFailurePause = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan CacheRemoveRetryDelay = TimeSpan.Zero;
        private const int CacheRemoveRetryCount = 1;

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;
        private DateTime _distributedCacheUnavailableUntilUtc = DateTime.MinValue;

        /// <summary>
        /// Кэш содержимого репозитория Philadelphus на базе распределенного кэша.
        /// </summary>
        /// <param name="distributedCache">Распределенный кэш.</param>
        /// <param name="logger">Сервис логгирования.</param>
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
        /// Прочитать агрегаты рабочих деревьев из кэша Redis.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных.</param>
        /// <returns>Коллекция агрегатов рабочих деревьев или null, если кэш пуст или недоступен.</returns>
        public IReadOnlyCollection<WorkingTree>? SelectTreeAggregatesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            InfrastructureCacheReadContext? cacheReadContext)
        {
            return GetCache<WorkingTree>(
                GetTreeAggregatesCacheKey(dataStorageUuid, uuids),
                cacheReadContext);
        }

        /// <summary>
        /// Записать агрегаты рабочих деревьев в кэш Redis.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        /// <param name="items">Коллекция агрегатов рабочих деревьев.</param>
        public void SetTreeAggregatesCache(
            Guid dataStorageUuid,
            Guid[]? uuids,
            IEnumerable<WorkingTree> items)
        {
            SetCache(
                GetTreeAggregatesCacheKey(dataStorageUuid, uuids),
                items,
                CloneTreeAggregateForCache);
        }

        /// <summary>
        /// Удалить кэш рабочих деревьев из Redis.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        public void InvalidateTrees(Guid dataStorageUuid, Guid[]? uuids)
        {
            RemoveCache(GetTreeAggregatesCacheBaseKey(dataStorageUuid, uuids));
        }

        /// <summary>
        /// Удалить кэш содержимого рабочего дерева из Redis.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="treeUuid">Идентификатор рабочего дерева.</param>
        public void InvalidateTreeContent(Guid dataStorageUuid, Guid treeUuid)
        {
            InvalidateTrees(dataStorageUuid, new[] { treeUuid });
        }

        /// <summary>
        /// Получить коллекцию из кэша.
        /// </summary>
        /// <typeparam name="T">Тип сущности инфраструктуры.</typeparam>
        /// <param name="cacheKey">Ключ кэша.</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных.</param>
        /// <returns>Коллекция сущностей инфраструктуры или null, если кэш пуст или недоступен.</returns>
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
        /// Записать коллекцию в кэш.
        /// </summary>
        /// <typeparam name="T">Тип сущности инфраструктуры.</typeparam>
        /// <param name="cacheKey">Ключ кэша.</param>
        /// <param name="items">Коллекция сущностей инфраструктуры.</param>
        /// <param name="clone">Функция подготовки сущности к кэшированию.</param>
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
        /// Попробовать прочитать строку из распределенного кэша.
        /// </summary>
        /// <param name="cacheKey">Ключ кэша.</param>
        /// <param name="cacheReadContext">Контекст чтения кэшируемых данных.</param>
        /// <param name="cachedJson">Содержимое кэша.</param>
        /// <returns>Признак успешного обращения к кэшу.</returns>
        private bool TryGetCacheString(
            string cacheKey,
            InfrastructureCacheReadContext? cacheReadContext,
            out string? cachedJson)
        {
            cachedJson = null;

            try
            {
                cachedJson = _distributedCache.GetString(cacheKey);
                if (cachedJson == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MarkDistributedCacheUnavailable(ex, "чтения", cacheKey);
                cacheReadContext?.MarkDistributedCacheUnavailable();
                return false;
            }
        }

        /// <summary>
        /// Проверить временную недоступность распределенного кэша.
        /// </summary>
        /// <returns>Признак временной недоступности распределенного кэша.</returns>
        private bool IsDistributedCacheTemporarilyUnavailable()
        {
            return DateTime.UtcNow < _distributedCacheUnavailableUntilUtc;
        }

        /// <summary>
        /// Отметить распределенный кэш временно недоступным.
        /// </summary>
        /// <param name="ex">Исключение операции кэша.</param>
        /// <param name="operationName">Название операции.</param>
        /// <param name="cacheKey">Ключ кэша.</param>
        private void MarkDistributedCacheUnavailable(Exception ex, string operationName, string cacheKey)
        {
            _distributedCacheUnavailableUntilUtc = DateTime.UtcNow.Add(DistributedCacheFailurePause);
            _logger.Warning(
                ex,
                $"Ошибка {operationName} кэша '{cacheKey}'. Кэш временно отключен, данные будут загружены из хранилища.");
        }

        /// <summary>
        /// Удалить запись кэша.
        /// </summary>
        /// <param name="cacheBaseKey">Базовый ключ кэша.</param>
        private void RemoveCache(string cacheBaseKey)
        {
            if (IsDistributedCacheTemporarilyUnavailable())
            {
                return;
            }

            for (var attempt = 1; attempt <= CacheRemoveRetryCount; attempt++)
            {
                try
                {
                    _distributedCache.Remove(cacheBaseKey);
                    return;
                }
                catch (Exception ex)
                {
                    if (attempt == CacheRemoveRetryCount)
                    {
                        MarkDistributedCacheUnavailable(ex, "удаления", cacheBaseKey);
                        return;
                    }

                    Thread.Sleep(CacheRemoveRetryDelay);
                }
            }
        }

        /// <summary>
        /// Получить ключ кэша агрегатов рабочих деревьев.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        /// <returns>Ключ кэша.</returns>
        private string GetTreeAggregatesCacheKey(Guid dataStorageUuid, Guid[]? uuids)
        {
            return GetTreeAggregatesCacheBaseKey(dataStorageUuid, uuids);
        }

        /// <summary>
        /// Получить базовый ключ кэша агрегатов рабочих деревьев.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="uuids">Идентификаторы рабочих деревьев.</param>
        /// <returns>Базовый ключ кэша.</returns>
        private static string GetTreeAggregatesCacheBaseKey(Guid dataStorageUuid, Guid[]? uuids)
        {
            return GetCacheBaseKey(dataStorageUuid, "tree-aggregates", uuids);
        }

        /// <summary>
        /// Получить ключ кэша.
        /// </summary>
        /// <param name="dataStorageUuid">Идентификатор хранилища данных.</param>
        /// <param name="entityGroup">Группа сущностей.</param>
        /// <param name="uuids">Идентификаторы сущностей или владельцев.</param>
        /// <returns>Ключ кэша.</returns>
        private static string GetCacheBaseKey(Guid dataStorageUuid, string entityGroup, Guid[]? uuids)
        {
            var normalizedUuids = uuids == null || uuids.Length == 0
                ? "all"
                : string.Join("-", uuids.OrderBy(x => x).Select(x => x.ToString("N")));

            return $"Philadelphus:CoreDomain:v2:storage:{dataStorageUuid:N}:{entityGroup}:{normalizedUuids}";
        }

        /// <summary>
        /// Создать параметры записи распределенного кэша.
        /// </summary>
        /// <returns>Параметры записи кэша.</returns>
        private static DistributedCacheEntryOptions CreateDistributedCacheEntryOptions()
        {
            return new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            };
        }

        /// <summary>
        /// Подготовить агрегат рабочего дерева к кэшированию.
        /// </summary>
        /// <param name="item">Агрегат рабочего дерева.</param>
        /// <returns>Агрегат без обратных навигационных свойств.</returns>
        private static WorkingTree CloneTreeAggregateForCache(WorkingTree item)
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
                OwnDataStorageUuid = item.OwnDataStorageUuid,
                ContentRoot = item.ContentRoot == null ? null! : CloneForCache(item.ContentRoot),
                ContentNodes = item.ContentNodes.Select(CloneForCache).ToList(),
                ContentLeaves = item.ContentLeaves.Select(CloneForCache).ToList(),
                ContentAttributes = item.ContentAttributes.Select(CloneForCache).ToList()
            };
        }

        /// <summary>
        /// Подготовить сущность корня к кэшированию.
        /// </summary>
        /// <param name="item">Сущность корня.</param>
        /// <returns>Сущность без навигационных свойств.</returns>
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
        /// Подготовить сущность узла к кэшированию.
        /// </summary>
        /// <param name="item">Сущность узла.</param>
        /// <returns>Сущность без навигационных свойств.</returns>
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
        /// Подготовить сущность листа к кэшированию.
        /// </summary>
        /// <param name="item">Сущность листа.</param>
        /// <returns>Сущность без навигационных свойств.</returns>
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
        /// Подготовить сущность атрибута к кэшированию.
        /// </summary>
        /// <param name="item">Сущность атрибута.</param>
        /// <returns>Сущность без навигационных свойств.</returns>
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
        /// Клонировать информацию аудита.
        /// </summary>
        /// <param name="auditInfo">Информация аудита.</param>
        /// <returns>Копия информации аудита.</returns>
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
