using AutoMapper;
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
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Serilog;
using System.Diagnostics;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Основной сервис для работы с репозиторием и его элементами
    /// </summary>
    public class PhiladelphusRepositoryService : IPhiladelphusRepositoryService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Основной сервис для работы с репозиторием и его элементами
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Сервис логгирования</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        public PhiladelphusRepositoryService(
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);

            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить содержимое репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="force">Признак принудительного получения из хранилища данных</param>
        /// <returns>Репозиторий с содержимым</returns>
        public PhiladelphusRepositoryModel GetShrubContent(PhiladelphusRepositoryModel repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            var shrub = repository.ContentShrub
                ?? throw new InvalidOperationException("Рабочий кустарник не инициализирован");

            repository.ContentShrub.ContentWorkingTrees.Clear();

            var sw = Stopwatch.StartNew();
            var result = GetShrubContentForce(repository);

            sw.Stop();

            SendContentLoadNotification("Содержимое репозитория", repository.Name, "БД", sw.Elapsed);

            InitSystemWorkingTree(repository.ContentShrub);

            return result;
        }

        /// <summary>
        /// Асинхронно получить содержимое репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="force">Признак принудительного получения из хранилища данных</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Репозиторий с содержимым</returns>
        public Task<PhiladelphusRepositoryModel> GetShrubContentAsync(
            PhiladelphusRepositoryModel repository,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(repository);

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return GetShrubContent(repository);
            }, cancellationToken);
        }

        /// <summary>
        /// Получить элементы рабочего дерева
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="force">Признак принудительного получения из хранилища данных</param>
        /// <returns>Дерево с содержимым</returns>
        public WorkingTreeModel GetWorkingTreeContent(WorkingTreeModel tree)
        {
            ArgumentNullException.ThrowIfNull(tree);

            var sw = Stopwatch.StartNew();
            tree.ContentRoot = null;

            var result = GetWorkingTreeContentForce(tree);

            sw.Stop();

            SendContentLoadNotification("Содержимое рабочего дерева", tree.Name, "БД", sw.Elapsed);

            return result;
        }

        /// <summary>
        /// Асинхронно получить элементы рабочего дерева
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="force">Признак принудительного получения из хранилища данных</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Дерево с содержимым</returns>
        public Task<WorkingTreeModel> GetWorkingTreeContentAsync(
            WorkingTreeModel tree,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tree);

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return GetWorkingTreeContent(tree);
            }, cancellationToken);
        }

        /// <summary>
        /// Распределить содержимое рабочего дерева без атрибутов по доменной модели
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="dbRoots">Сущности корней рабочего дерева</param>
        /// <param name="dbNodes">Сущности узлов рабочего дерева</param>
        /// <param name="dbLeaves">Сущности листьев рабочего дерева</param>
        private void ApplyWorkingTreeContentWithoutAttributes(
            WorkingTreeModel tree,
            IReadOnlyCollection<TreeRoot> dbRoots,
            IReadOnlyCollection<TreeNode> dbNodes,
            IReadOnlyCollection<TreeLeave> dbLeaves)
        {
            if (dbRoots.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Рабочее дерево '{tree.Name}' [{tree.Uuid}] должно иметь ровно один корень. Получено: {dbRoots.Count}.");
            }

            var dbRoot = dbRoots.Single();
            var root = _mapper.MapTreeRoot(dbRoot, tree, _notificationService, new EmptyPropertiesPolicy<TreeRootModel>());

            if (root == null)
            {
                return;
            }

            tree.ContentRoot = root;
            SetModelState(root, State.SavedOrLoaded);

            var nodes = _mapper.MapTreeNodes(dbNodes, new[] { tree.ContentRoot }, tree.ContentRoot.OwningWorkingTree, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            var allNodes = new List<TreeNodeModel>();
            while (nodes.Any())
            {
                allNodes.AddRange(nodes);
                nodes = _mapper.MapTreeNodes(dbNodes, nodes, tree.ContentRoot.OwningWorkingTree, _notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());
            }

            var allLeaves = _mapper.MapTreeLeaves(dbLeaves, allNodes, tree, _notificationService, new EmptyPropertiesPolicy<TreeLeaveModel>());

            tree.UnavailableNames.Add(root.Name);
            foreach (var item in allNodes)
            {
                tree.UnavailableNames.Add(item.Name);
            }
            foreach (var item in allLeaves)
            {
                tree.UnavailableNames.Add(item.Name);
            }

            var nodesByParent = allNodes
                .Where(x => x.Parent != null)
                .GroupBy(x => x.Parent.Uuid)
                .ToDictionary(x => x.Key, x => x.ToList());
            var leavesByParent = allLeaves
                .Where(x => x.Parent != null)
                .GroupBy(x => x.Parent.Uuid)
                .ToDictionary(x => x.Key, x => x.ToList());

            DistributeTreeRootDescendants(root, nodesByParent, leavesByParent);
        }

        private TreeRootModel DistributeTreeRootDescendants(
            TreeRootModel root,
            IReadOnlyDictionary<Guid, List<TreeNodeModel>> nodesByParent,
            IReadOnlyDictionary<Guid, List<TreeLeaveModel>> leavesByParent)
        {
            root.ChildNodes.Clear();

            if (nodesByParent.TryGetValue(root.Uuid, out var childNodes))
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                root.ChildNodes.AddRange(childNodes);

                foreach (var child in childNodes)
                {
                    DistributeTreeNodeDescendants(child, nodesByParent, leavesByParent);
                }
            }

            return root;
        }

        private TreeNodeModel DistributeTreeNodeDescendants(
            TreeNodeModel node,
            IReadOnlyDictionary<Guid, List<TreeNodeModel>> nodesByParent,
            IReadOnlyDictionary<Guid, List<TreeLeaveModel>> leavesByParent)
        {
            node.ChildNodes.Clear();
            node.ChildLeaves.Clear();

            if (nodesByParent.TryGetValue(node.Uuid, out var childNodes))
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                node.ChildNodes.AddRange(childNodes);

                foreach (var child in childNodes)
                {
                    DistributeTreeNodeDescendants(child, nodesByParent, leavesByParent);
                }
            }

            if (leavesByParent.TryGetValue(node.Uuid, out var childLeaves))
            {
                foreach (var child in childLeaves)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                node.ChildLeaves.AddRange(childLeaves);

                foreach (var child in childLeaves)
                {
                }
            }

            return node;
        }

        private IEnumerable<ElementAttributeModel> DistributeAttributes(IAttributeOwnerModel attributeOwner, IEnumerable<ElementAttributeModel> allAttributes, IEnumerable<TreeNodeModel> allDataTypes, IEnumerable<TreeLeaveModel> allValues)
        {
            attributeOwner.ClearAttributes();

            foreach (var attribute in allAttributes.Where(x => x.Owner.Uuid == attributeOwner.Uuid))
            {
                attributeOwner.AddAttribute(attribute);
                SetModelState(attribute, State.SavedOrLoaded);
            }

            return attributeOwner.Attributes;
        }

        #endregion


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
                    var dbTrees = SelectTreeAggregatesForce(dataStorage, treesUuids);
                    var trees = _mapper.MapWorkingTrees(dbTrees, repository.DataStorages, repository.ContentShrub, _notificationService, new EmptyPropertiesPolicy<WorkingTreeModel>());
                    var dbTreesByUuid = dbTrees.ToDictionary(x => x.Uuid);
                    var loadedTrees = new List<WorkingTreeModel>();

                    foreach (var tree in trees?.Where(x => x.OwningRepository.Uuid == repository.Uuid))
                    {
                        tree.UnavailableNames.Add(tree.Name);

                        repository.ContentShrub.ContentWorkingTrees.Add(tree);

                        if (dbTreesByUuid.TryGetValue(tree.Uuid, out var dbTree))
                        {
                            LoadWorkingTreeContentWithoutAttributes(tree, dbTree);
                        }

                        SetModelState(tree, State.SavedOrLoaded);
                        loadedTrees.Add(tree);
                    }

                    // Получение атрибутов всех элементов деревьев из БД
                    foreach (var tree in loadedTrees)
                    {
                        if (dbTreesByUuid.TryGetValue(tree.Uuid, out var dbTree))
                        {
                            LoadWorkingTreeAttributes(tree, dbTree);
                        }
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
            tree.ContentRoot = null!;
            var dbTree = SelectTreeAggregatesForce(tree.OwnDataStorage, new[] { tree.Uuid }).SingleOrDefault();
            if (dbTree == null)
            {
                return tree;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            LoadWorkingTreeContentWithoutAttributes(tree, dbTree);
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Применение агрегата рабочего дерева без атрибутов к доменной модели выполнено за {sw.ElapsedMilliseconds} мс. Дерево: '{tree.Name}' [{tree.Uuid}].",
                criticalLevel: NotificationCriticalLevelModel.Info);
            sw.Restart();

            LoadWorkingTreeAttributes(tree, dbTree);

            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"Привязка атрибутов выполнена за {sw.ElapsedMilliseconds} мс. Дерево: '{tree.Name}' [{tree.Uuid}].",
                criticalLevel: NotificationCriticalLevelModel.Info);

            return tree;
        }

        /// <summary>
        /// Загрузить атрибуты рабочего дерева из агрегата.
        /// </summary>
        /// <param name="tree">Рабочее дерево.</param>
        /// <param name="dbTree">Агрегат рабочего дерева из хранилища данных.</param>
        private void LoadWorkingTreeAttributes(
            WorkingTreeModel tree,
            WorkingTree dbTree)
        {
            ApplyWorkingTreeAttributes(tree, dbTree.ContentAttributes.ToList());
        }

        private void ApplyWorkingTreeAttributes(
            WorkingTreeModel tree,
            IReadOnlyCollection<ElementAttribute> dbAttributes)
        {

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
            var allAttributes = _mapper.MapAttributes(dbAttributes, owners, allShrubNodes, allShrubLeaves, tree, _notificationService, AttributePolicyBuilder.CreateDefault(_notificationService));

            // Распределение атрибутов по владельцам
            foreach (var item in owners)
            {
                DistributeAttributes(item, allAttributes, allShrubNodes, allShrubLeaves);
            }
        }

        /// <summary>
        /// Загрузить содержимое рабочего дерева без атрибутов из агрегата.
        /// </summary>
        /// <param name="tree">Рабочее дерево.</param>
        /// <param name="dbTree">Агрегат рабочего дерева из хранилища данных.</param>
        private void LoadWorkingTreeContentWithoutAttributes(
            WorkingTreeModel tree,
            WorkingTree dbTree)
        {
            var dbRoots = dbTree.ContentRoot == null
                ? Array.Empty<TreeRoot>()
                : new[] { dbTree.ContentRoot };

            ApplyWorkingTreeContentWithoutAttributes(
                tree,
                dbRoots,
                dbTree.ContentNodes.ToList(),
                dbTree.ContentLeaves.ToList());
        }

        /// <summary>
        /// Принудительно прочитать агрегаты рабочих деревьев из хранилища данных.
        /// </summary>
        /// <param name="dataStorage">Хранилище данных.</param>
        /// <param name="uuids">Идентификаторы деревьев.</param>
        /// <returns>Коллекция агрегатов рабочих деревьев.</returns>
        private IReadOnlyCollection<WorkingTree> SelectTreeAggregatesForce(IDataStorageModel dataStorage, Guid[]? uuids)
        {
            var items = dataStorage.ShrubMembersInfrastructureRepository
                .SelectTreeAggregates(uuids)
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
                .SelectTreeAggregates(new Guid[] { WorkingTreeModel.SystemBaseUuid });
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

        #region [ Create + Add ]

        /// <summary>
        /// Создать корень и добавить родителю
        /// </summary>
        /// <param name="owner">Родитель</param>
        /// <param name="dataStorage">Хранилище</param>
        /// <returns>Корень</returns>
        public WorkingTreeModel CreateWorkingTree(PhiladelphusRepositoryModel owner, IDataStorageModel dataStorage, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания рабочего дерева. Владелец - '{(owner as IMainEntityModel).Name}' [{(owner as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                var result = new WorkingTreeModel(
                    Guid.CreateVersion7(),
                    dataStorage,
                    owner.ContentShrub,
                    _notificationService,
                    new EmptyPropertiesPolicy<WorkingTreeModel>());

                if (needAutoName)
                {
                    result.AssignAutoName();
                }

                owner.ContentShrub.ContentWorkingTrees.Add(result);
                owner.ContentShrub.ContentWorkingTreesUuids.Add(result.Uuid);

                SetModelState(result, State.Initialized);
                SetModelState(owner, State.Changed);


                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Создание рабочего дерева '{result.Name}' успешно выполнено.",
                        criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания рабочего дерева.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания рабочего дерева. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать корень и добавить родителю
        /// </summary>
        /// <param name="parentElement">Родитель</param>
        /// <param name="dataStorage">Хранилище</param>
        /// <returns>Корень</returns>
        public TreeRootModel CreateTreeRoot(WorkingTreeModel owner, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания корня. Владелец - '{(owner as IMainEntityModel).Name}' [{(owner as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                var result = new TreeRootModel(
                    Guid.CreateVersion7(),
                    owner,
                    _notificationService,
                    new EmptyPropertiesPolicy<TreeRootModel>());

                if (needAutoName)
                {
                    result.AssignAutoName();
                }

                SetModelState(result, State.Initialized);

                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Создание корня '{result.Name}' успешно выполнено.",
                        criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания корня.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания корня. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать узел и добавить родителю
        /// </summary>
        /// <param name="parent">Родитель</param>
        /// <returns>Узел</returns>
        public TreeNodeModel CreateTreeNode(IParentModel parent, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания узла. Родитель - '{(parent as IMainEntityModel).Name}' [{(parent as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                var result = new TreeNodeModel(
                    Guid.CreateVersion7(),
                    parent,
                    (parent as IWorkingTreeMemberModel)?.OwningWorkingTree,
                    _notificationService,
                    new EmptyPropertiesPolicy<TreeNodeModel>());

                if (needAutoName)
                {
                    result.AssignAutoName();
                }

                if (parent is TreeNodeModel node)
                    node.ChildNodes.Add(result);
                if (parent is TreeRootModel root)
                    root.ChildNodes.Add(result);

                SetModelState(result, State.Initialized);

                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Создание узла '{result.Name}' успешно выполнено.",
                        criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания узла.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания узла. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать лист и добавить родителю
        /// </summary>
        /// <param name="parent">Родитель</param>
        /// <returns>Лист</returns>
        public TreeLeaveModel CreateTreeLeave(TreeNodeModel parent, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания листа. Родитель - '{(parent as IMainEntityModel).Name}' [{(parent as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                TreeLeaveModel result = null;
                if (parent is SystemBaseTreeNodeModel sbn)
                {
                    result = new SystemBaseTreeLeaveModel(
                        Guid.CreateVersion7(),
                        sbn,
                        sbn.OwningWorkingTree,
                        sbn.SystemBaseType,
                        _notificationService,
                        new EmptyPropertiesPolicy<TreeLeaveModel>());
                }
                else
                {
                    result = new TreeLeaveModel(
                        Guid.CreateVersion7(),
                        parent,
                        parent.OwningWorkingTree,
                        _notificationService,
                        new EmptyPropertiesPolicy<TreeLeaveModel>());
                }

                if (needAutoName)
                {
                    result.AssignAutoName();
                }

                parent.ChildLeaves.Add(result);

                SetModelState(result, State.Initialized);

                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                       $"Создание листа '{result.Name}' успешно выполнено.",
                       criticalLevel: NotificationCriticalLevelModel.Ok);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания листа.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания листа. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать атрибут и добавить владельцу
        /// </summary>
        /// <param name="owner">Владелец</param>
        /// <returns>Атрибут</returns>
        public ElementAttributeModel CreateElementAttribute(IAttributeOwnerModel owner, bool needAutoName = true, bool withoutInfoNotifications = false)
        {
            try
            {
                if (withoutInfoNotifications == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Начало создания атрибута. Владелец - '{(owner as IMainEntityModel).Name}' [{(owner as IMainEntityModel).Uuid}].",
                        criticalLevel: NotificationCriticalLevelModel.Info);
                }

                if (owner is TreeLeaveModel)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Ошибка создания атрибута. Изменение перечня атрибутов листов не допускается.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return null;
                }

                var uuid = Guid.CreateVersion7();

                if (owner is IWorkingTreeMemberModel wtm)
                {
                    var result = new ElementAttributeModel(
                        uuid,
                        owner,
                        uuid,
                        owner,
                        wtm.OwningWorkingTree,
                        _notificationService,
                        AttributePolicyBuilder.CreateDefault(_notificationService))
                    {
                        ValueType = wtm.OwningShrub.SystemBaseWorkingTree.GetAllNodesRecursive().SingleOrDefault(x => x is SystemBaseTreeNodeModel sbn && sbn.SystemBaseType == SystemBaseType.STRING)
                    };

                    if (needAutoName)
                    {
                        result.AssignAutoName();
                    }

                    if (owner.AddAttribute(result))
                    {
                        SetModelState(result, State.Initialized);

                        if (withoutInfoNotifications == false)
                        {
                            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                                $"Создание атрибута '{result.Name}' успешно выполнено.",
                                criticalLevel: NotificationCriticalLevelModel.Ok);
                        }
                    }
                    else
                    {
                        _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                            $"Ошибка добавления атрибута владельцу '{(owner as IMainEntityModel).Name}' [{(owner as IMainEntityModel).Uuid}].",
                            criticalLevel: NotificationCriticalLevelModel.Error);
                    }

                    return result;
                }
                _logger.Warning("Попытка добавления атрибута элементу, НЕ являющумяся участником рабочего дерева.");
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания атрибута. Атрибут можно добавить только участнику рабочего дерева.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка создания атрибута.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка создания атрибута. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        /// <summary>
        /// Мягкое удаление элемента репозитория
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <returns></returns>
        public bool SoftDeleteShrubMember(IContentModel element)
        {
            try
            {
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Начало удаления элемента. Элемент - '{(element as IMainEntityModel).Name}' [{(element as IMainEntityModel).Uuid}].",
                    criticalLevel: NotificationCriticalLevelModel.Warning);

                if (element == null)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Удаление элемента невозможно, элемент не выбран, операция не выполнена. Выберите элемент для удаления и повторите попытку.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return false;
                }

                if (element is ElementAttributeModel ea && ea.Owner is TreeLeaveModel)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                        $"Удаление элемента невозможно. Изменение перечня атрибутов листов не допускается.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return false;
                }

                long result = 0;
                if (element is IMainEntityWritableModel me)
                {
                    SetModelState(me, State.ForSoftDelete);
                }
                if (element is WorkingTreeModel wt)
                {
                    wt.OwningShrub.ContentWorkingTreesUuids.Remove(wt.Uuid);
                    SetModelState(wt.OwningShrub, State.Changed);
                    SetModelState(wt.ContentRoot, State.ForSoftDelete);
                }
                if (element is TreeRootModel r)
                {
                    SetModelState(r.OwningWorkingTree, State.ForSoftDelete);
                    r.OwningShrub.ContentWorkingTreesUuids.Remove(r.Uuid);
                    SetModelState(r.OwningShrub, State.Changed);
                }

                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Удаление элемента успешно выполнено. Изменения применяются после сохранения, если удаление не требуется - выполните обновление данных из хранилища.",
                    criticalLevel: NotificationCriticalLevelModel.Ok);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка удаления элемента.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                    $"Ошибка удаления элемента. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        #endregion

        #region [ Temp ]



        #endregion

        #region [ Private methods ]

        /// <summary>
        /// Отправить уведомление о получении содержимого
        /// </summary>
        /// <param name="contentName">Название содержимого</param>
        /// <param name="entityName">Название сущности</param>
        /// <param name="sourceName">Название источника данных</param>
        /// <param name="elapsed">Время выполнения операции</param>
        private void SendContentLoadNotification(
            string contentName,
            string entityName,
            string sourceName,
            TimeSpan elapsed)
        {
            _notificationService.SendTextMessage<PhiladelphusRepositoryService>(
                $"{contentName} '{entityName}' получено. Источник - {sourceName}, время операции - {elapsed.TotalMilliseconds:0.###} мс.",
                criticalLevel: NotificationCriticalLevelModel.Info);
        }

        /// <summary>
        /// Изменить статус элемента
        /// </summary>
        /// <param name="model">Элемент</param>
        /// <param name="newState">Новый статус</param>
        private void SetModelState(IMainEntityWritableModel model, State newState)
        {
            model.SetState(newState);
        }


        private void PostProcessSavedEntities(IEnumerable<IMainEntityWritableModel> collection)
        {
            // Постобработка удаленных элементов
            var remItems = new List<IMainEntityWritableModel>();
            foreach (var item in collection.Where(x =>
            x.State == State.ForSoftDelete
            || x.State == State.ForHardDelete
            || x.State == State.SoftDeleted))
            {
                if (item is IContentModel c)
                {
                    remItems.Add(item);
                }
                if (item is IChildrenModel ch)
                {
                    remItems.Add(item);
                }
            }
            foreach (var item in remItems)
            {
                if (item is IContentModel c)
                {
                    c.Owner.RemoveContent(c);
                }
                if (item is IChildrenModel ch)
                {
                    ch.Parent.RemoveChild(ch);
                }
            }

            // Актуализация статуса остальных
            foreach (var item in collection.Where(x =>
            x.State == State.Initialized
            || x.State == State.Changed))
            {
                SetModelState(item, State.SavedOrLoaded);
            }
        }

        #endregion
    }
}
