using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Часть сервиса для работы с хранилищем данных
    /// </summary>
    public partial class PhiladelphusRepositoryService
    {
        /// <summary>
        /// Принудительно получить содержимое репозитория из хранилища данных
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с содержимым</returns>
        private PhiladelphusRepositoryModel GetShrubContentForce(PhiladelphusRepositoryModel repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repository.ContentShrub);

            repository.ContentShrub.ContentWorkingTrees.Clear();

            foreach (var dataStorage in repository.DataStorages ?? Enumerable.Empty<IDataStorageModel>())
            {
                if (dataStorage.IsAvailable)
                {
                    var treesUuids = repository.ContentShrub.ContentWorkingTreesUuids?.ToArray();
                    var dbTrees = SelectTreesForce(dataStorage, treesUuids);
                    RefreshTreesCache(dataStorage.Uuid, treesUuids, dbTrees);
                    var trees = _mapper.MapWorkingTrees(dbTrees, repository.DataStorages, repository.ContentShrub, _notificationService, new EmptyPropertiesPolicy<WorkingTreeModel>());

                    foreach (var tree in trees?.Where(x => x.OwningRepository.Uuid == repository.Uuid))
                    {
                        tree.UnavailableNames.Add(tree.Name);

                        repository.ContentShrub.ContentWorkingTrees.Add(tree);

                        GetWorkingTreeContentWithoutAttributesForce(tree);

                        SetModelState(tree, State.SavedOrLoaded);
                    }

                    // Получение атрибутов всех элементов деревьев из БД
                    foreach (var tree in repository.ContentShrub.ContentWorkingTrees)
                    {
                        GetWorkingTreeContentForce(tree);
                    }
                }
            }

            InitSystemWorkingTree(repository.ContentShrub);

            SetModelState(repository, State.SavedOrLoaded);

            return repository;
        }

        /// <summary>
        /// Принудительно получить элементы рабочего дерева из хранилища данных
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <returns>Дерево с содержимым</returns>
        private WorkingTreeModel GetWorkingTreeContentForce(WorkingTreeModel tree)
        {
            ArgumentNullException.ThrowIfNull(tree);

            // Получение дерева без атрибутов
            tree.ContentRoot = null;
            GetWorkingTreeContentWithoutAttributesForce(tree);

            // Получение полных списков типов данных (узлов) и значений (листов) атрибутов со всего кустарника
            var allShrubNodes = tree.OwningShrub.ContentWorkingTrees.SelectMany(x => x.GetAllNodesRecursive() ?? new List<TreeNodeModel>()).ToList();
            var allShrubLeaves = tree.OwningShrub.ContentWorkingTrees.SelectMany(x => x.GetAllLeavesRecursive() ?? new List<TreeLeaveModel>()).ToList();

            // Формирование списка владельцев атрибутов
            var owners = new List<IAttributeOwnerModel>();
            owners.Add(tree);
            if (tree.ContentRoot != null)
            {
                owners.Add(tree.ContentRoot);
            }

            owners.AddRange(tree.GetAllNodesRecursive() ?? Enumerable.Empty<TreeNodeModel>());
            owners.AddRange(tree.GetAllLeavesRecursive() ?? Enumerable.Empty<TreeLeaveModel>());

            // Получение атрибутов всех элементов дерева из БД
            var dbAttributes = SelectAttributesForce(tree.OwnDataStorage, new[] { tree.Uuid });
            RefreshAttributesCache(tree.OwnDataStorage.Uuid, new[] { tree.Uuid }, dbAttributes);
            var allAttributes = _mapper.MapAttributes(dbAttributes, owners, allShrubNodes, allShrubLeaves, tree, _notificationService, AttributePolicyBuilder.CreateDefault(_notificationService));

            // Распределение атрибутов по владельцам
            foreach (var item in owners)
            {
                DistributeAttributes(item, allAttributes, allShrubNodes, allShrubLeaves);
            }

            return tree;
        }

        /// <summary>
        /// Принудительно получить элементы рабочего дерева без атрибутов из хранилища данных
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <returns>Дерево с содержимым без атрибутов</returns>
        private WorkingTreeModel GetWorkingTreeContentWithoutAttributesForce(
            WorkingTreeModel tree)
        {
            ArgumentNullException.ThrowIfNull(tree);

            tree.ContentRoot = null!;

            if (tree.DataStorage.IsAvailable)
            {
                var treeUuids = new[] { tree.Uuid };
                var dbRoots = SelectRootsForce(tree.DataStorage, treeUuids);
                var dbNodes = SelectNodesForce(tree.DataStorage, treeUuids);
                var dbLeaves = SelectLeavesForce(tree.DataStorage, treeUuids);
                RefreshRootsCache(tree.DataStorage.Uuid, treeUuids, dbRoots);
                RefreshNodesCache(tree.DataStorage.Uuid, treeUuids, dbNodes);
                RefreshLeavesCache(tree.DataStorage.Uuid, treeUuids, dbLeaves);

                ApplyWorkingTreeContentWithoutAttributes(tree, dbRoots, dbNodes, dbLeaves);
            }

            return tree;
        }

        /// <summary>
        /// Прочитать рабочие деревья из хранилища данных
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="uuids">Идентификаторы деревьев</param>
        /// <returns>Коллекция сущностей рабочих деревьев</returns>
        private IReadOnlyCollection<WorkingTree> SelectTreesForce(IDataStorageModel dataStorage, Guid[]? uuids)
        {
            var items = dataStorage.ShrubMembersInfrastructureRepository
                .SelectTrees(uuids)
                .ToList();

            return items;
        }

        /// <summary>
        /// Прочитать корни рабочих деревьев из хранилища данных
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей корней</returns>
        private IReadOnlyCollection<TreeRoot> SelectRootsForce(IDataStorageModel dataStorage, Guid[] owningTreesUuids)
        {
            var items = dataStorage.ShrubMembersInfrastructureRepository
                .SelectRoots(owningTreesUuids)
                .ToList();

            return items;
        }

        /// <summary>
        /// Прочитать узлы рабочих деревьев из хранилища данных
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей узлов</returns>
        private IReadOnlyCollection<TreeNode> SelectNodesForce(IDataStorageModel dataStorage, Guid[] owningTreesUuids)
        {
            var items = dataStorage.ShrubMembersInfrastructureRepository
                .SelectNodes(owningTreesUuids)
                .ToList();

            return items;
        }

        /// <summary>
        /// Прочитать листья рабочих деревьев из хранилища данных
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей листьев</returns>
        private IReadOnlyCollection<TreeLeave> SelectLeavesForce(IDataStorageModel dataStorage, Guid[] owningTreesUuids)
        {
            var items = dataStorage.ShrubMembersInfrastructureRepository
                .SelectLeaves(owningTreesUuids)
                .ToList();

            return items;
        }

        /// <summary>
        /// Прочитать атрибуты рабочих деревьев из хранилища данных
        /// </summary>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="owningTreesUuids">Идентификаторы рабочих деревьев</param>
        /// <returns>Коллекция сущностей атрибутов</returns>
        private IReadOnlyCollection<ElementAttribute> SelectAttributesForce(IDataStorageModel dataStorage, Guid[] owningTreesUuids)
        {
            var items = dataStorage.ShrubMembersInfrastructureRepository
                .SelectAttributes(owningTreesUuids)
                .ToList();

            return items;
        }

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий)
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(ref PhiladelphusRepositoryModel repository, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (repository == null)
                return 0;

            var originalState = repository.State;

            try
            {
                // Уведомление
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Начало сохранения репозитория. Репозиторий: '{repository.Name}'",
                    criticalLevel: NotificationCriticalLevelModel.Info);

                // Преобразование в сущность БД
                long result = 0;
                var infrastructure = repository.OwnDataStorage?.PhiladelphusRepositoriesInfrastructureRepository
                    ?? throw new InvalidOperationException("Инфраструктура хранилища не инициализирована");

                var entity = _mapper.Map<PhiladelphusRepository>(repository);
                var storage = repository.OwnDataStorage;

                // Сохранение изменений
                switch (repository.State)
                {
                    case State.Initialized:
                        result += infrastructure.InsertRepository(entity);
                        break;
                    case State.Changed:
                        result += infrastructure.UpdateRepository(entity);
                        break;
                    case State.ForSoftDelete:
                        result += infrastructure.SoftDeleteRepository(entity);
                        break;
                    default:
                        _logger.Warning($"Неизвестное состояние репозитория: {repository.State}");
                        break;
                }

                // Возвращение данных, генерируемых инфраструктурой
                repository.AuditInfo = _mapper.Map<AuditInfoModel>(entity.AuditInfo);

                // Актуализация статуса
                SetModelState(repository, State.SavedOrLoaded);
                // TODO: Удалить?

                // Сохранение содержимого
                if (saveMode == SaveMode.WithContent ||
                    saveMode == SaveMode.WithContentAndMembers)
                {
                }

                // Сохранение участников
                if (saveMode == SaveMode.WithContentAndMembers)
                {
                    result += SaveChanges(repository.ContentShrub.ContentWorkingTrees, saveMode);
                }

                // Уведомление
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Сохранение репозитория успешно выполнено. Сохранено {result} элементов.",
                    criticalLevel: NotificationCriticalLevelModel.Ok);

                InvalidateCachedContent(repository);

                return result;
            }
            catch (Exception ex)
            {
                SetModelState(repository, originalState);
                _logger.Error(ex, $"Ошибка сохранения репозитория '{repository.Name}'");
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка сохранения репозитория: {ex.Message}",
                    criticalLevel: NotificationCriticalLevelModel.Error);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно сохранить изменения (репозиторий)
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Количество сохраненных изменений</returns>
        public Task<long> SaveChangesAsync(
            PhiladelphusRepositoryModel repository,
            SaveMode saveMode,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(repository);

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var repositoryCopy = repository;
                return SaveChanges(ref repositoryCopy, saveMode);
            }, cancellationToken);
        }

        /// <summary>
        /// Сохранить изменения (рабочие деревья)
        /// </summary>
        /// <param name="workingTrees">Коллекция рабочих деревьев</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<WorkingTreeModel> workingTrees, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (workingTrees == null || workingTrees.Count() == 0)
                return 0;

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения рабочих деревьев. Рабочие деревья: {string.Join(", ", workingTrees.Select(x => $"'{x.Name}' [{x.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in workingTrees.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = workingTrees.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertTrees(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateTrees(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteTrees(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(workingTrees);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(workingTrees.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(workingTrees.Select(x => x.ContentRoot), saveMode);
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение рабочих деревьев успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            InvalidateCachedContent(workingTrees);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (корни)
        /// </summary>
        /// <param name="treeRoots">Коллекция корней</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeRootModel> treeRoots, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (treeRoots == null || treeRoots.Count() == 0)
                return 0;

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения корней. Рабочие деревья: {string.Join(", ", treeRoots.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in treeRoots.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeRoots.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertRoots(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateRoots(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteRoots(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(treeRoots);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(treeRoots.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(treeRoots.SelectMany(x => x.ChildNodes), saveMode);
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение корней успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            InvalidateCachedContent(treeRoots);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (узлы)
        /// </summary>
        /// <param name="treeNodes">Коллекция узлов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeNodeModel> treeNodes, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (treeNodes == null || treeNodes.Count() == 0)
                return 0;

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения узлов. Рабочие деревья: {string.Join(", ", treeNodes.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in treeNodes.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeNodes.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertNodes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateNodes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteNodes(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(treeNodes);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(treeNodes.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(treeNodes.SelectMany(x => x.ChildNodes), saveMode);
                result += SaveChanges(treeNodes.SelectMany(x => x.ChildLeaves), saveMode);
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение узлов успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            InvalidateCachedContent(treeNodes);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (листы)
        /// </summary>
        /// <param name="treeLeaves">Коллекция листов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeLeaveModel> treeLeaves, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (treeLeaves == null || treeLeaves.Count() == 0)
                return 0;

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения листов. Рабочие деревья: {string.Join(", ", treeLeaves.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in treeLeaves.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeLeaves.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertLeaves(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateLeaves(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteLeaves(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(treeLeaves);

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent ||
                saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveContentChanges(treeLeaves.SelectMany(x => x.Attributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
            }

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение листов успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            InvalidateCachedContent(treeLeaves);

            return result;
        }

        /// <summary>
        /// Сохранить изменения (атрибуты элемента)
        /// </summary>
        /// <param name="elementAttributes">Коллекция атрибутов</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveContentChanges(IEnumerable<ElementAttributeModel> elementAttributes)
        {
            // Проверка исходных данных
            if (elementAttributes == null || elementAttributes.Count() == 0)
                return 0;

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Начало сохранения атрибутов. Рабочие деревья: {string.Join(", ", elementAttributes.Select(x => $"'{x.OwningWorkingTree.Name}' [{x.OwningWorkingTree.Uuid}].").Distinct())}",
                criticalLevel: NotificationCriticalLevelModel.Info);

            // Сохранение изменений
            long result = 0;
            long initCount = 0;
            long changedCount = 0;
            long deletedCount = 0;
            foreach (var infrastructure in elementAttributes.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = elementAttributes.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => initCount = infrastructure.InsertAttributes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.Changed,
                    dbCollection => changedCount = infrastructure.UpdateAttributes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => deletedCount = infrastructure.SoftDeleteAttributes(dbCollection),
                    ref result);
            }

            // Постобработка сохраненных элементов
            PostProcessSavedEntities(elementAttributes);

            // Уведомление
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Сохранение атрибутов успешно выполнено. Сохранено {initCount + changedCount + deletedCount} шт. - новых {initCount} шт., измененных {changedCount} шт., удаленных {deletedCount} шт.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            InvalidateCachedContent(elementAttributes);

            return result;
        }

        #endregion

        /// <summary>
        /// Инициализировать системное рабочее дерево базовых типов
        /// </summary>
        /// <param name="shrub">Кустарник</param>
        /// <returns>Признак инициализации системного рабочего дерева</returns>
        private bool InitSystemWorkingTree(ShrubModel shrub)
        {
            var existTree = shrub.ContentWorkingTrees.SingleOrDefault(x => x.Uuid == WorkingTreeModel.SystemBaseUuid);
            if (existTree != null)
                return false;

            var dbExistTrees = shrub.DataStorage.ShrubMembersInfrastructureRepository
                .SelectTrees(new Guid[] { WorkingTreeModel.SystemBaseUuid });
            existTree = _mapper.MapWorkingTrees(dbExistTrees, shrub.OwningRepository.DataStorages, shrub, _notificationService, new EmptyPropertiesPolicy<WorkingTreeModel>()).SingleOrDefault();

            if (existTree != null)
            {
                shrub.ContentWorkingTrees.Add(existTree);
                shrub.ContentWorkingTreesUuids.Add(existTree.Uuid);
                return true;
            }

            // TODO: ех. долг #61187115

            var tree = new WorkingTreeModel(
                uuid: WorkingTreeModel.SystemBaseUuid,
                dataStorage: shrub.DataStorage,
                owner: shrub,
                notificationService: _notificationService,
                new EmptyPropertiesPolicy<WorkingTreeModel>())
            {
                Name = "Системное рабочее дерево"
            };

            var root = new TreeRootModel(
                uuid: TreeRootModel.SystemBaseUuid,
                owner: tree,
                notificationService: _notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>())
            {
                Name = "Базовые типы данных"
            };

            tree.ContentRoot = root;

            var obj = new SystemBaseTreeNodeModel(root, tree, SystemBaseType.OBJECT, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            root.ChildNodes.Add(obj);

            var str = new SystemBaseTreeNodeModel(obj, tree, SystemBaseType.STRING, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            obj.ChildNodes.Add(str);

            var num = new SystemBaseTreeNodeModel(obj, tree, SystemBaseType.NUMERIC, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            obj.ChildNodes.Add(num);

            var integer = new SystemBaseTreeNodeModel(num, tree, SystemBaseType.INTEGER, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            num.ChildNodes.Add(integer);

            var flt = new SystemBaseTreeNodeModel(num, tree, SystemBaseType.FLOAT, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            num.ChildNodes.Add(flt);

            shrub.ContentWorkingTrees.Add(tree);
            shrub.ContentWorkingTreesUuids.Add(tree.Uuid);
            return true;
        }

        /// <summary>
        /// Сохранить изменения и вернуть данные аудита в доменные модели
        /// </summary>
        /// <typeparam name="TMainEntityModel">Тип доменной модели</typeparam>
        /// <typeparam name="TMainEntity">Тип сущности хранилища данных</typeparam>
        /// <param name="fullCollection">Полная коллекция доменных моделей</param>
        /// <param name="state">Статус сохраняемых моделей</param>
        /// <param name="persister">Делегат сохранения сущностей в хранилище данных</param>
        /// <param name="result">Счетчик сохраненных изменений</param>
        private void SaveAndReturnAuditInfo<TMainEntityModel, TMainEntity>(
            IEnumerable<TMainEntityModel> fullCollection,
            State state,
            Func<IEnumerable<TMainEntity>, long> persister,
            ref long result)
            where TMainEntityModel : IMainEntityWritableModel
            where TMainEntity : IMainEntity
        {
            // Проверки
            ArgumentOutOfRangeException.ThrowIfEqual(state, State.SavedOrLoaded);

            // Подготовка
            var collection = fullCollection?
                .Where(x => x != null && x.State == state)
                .ToList() ?? new List<TMainEntityModel>();

            if (collection.Count == 0)
                return;

            var dbCollection = _mapper.Map<List<TMainEntity>>(collection);

            // Сохранение в хранилище
            result += persister(dbCollection);

            // Возвращение полей аудита их хранилища
            foreach (var (src, dest) in collection.Zip(dbCollection, (src, dest) => (src, dest)))
            {
                src.AuditInfo = _mapper.Map<AuditInfoModel>(dest.AuditInfo);
            }
        }
    }
}
