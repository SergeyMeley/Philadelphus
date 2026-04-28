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
using Philadelphus.Infrastructure.Cache.Context;
using Philadelphus.Infrastructure.Cache.RepositoryInterfaces;
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
    public partial class PhiladelphusRepositoryService : IPhiladelphusRepositoryService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IPhiladelphusRepositoryContentCache _contentCache;

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Основной сервис для работы с репозиторием и его элементами
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Сервис логгирования</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        /// <param name="contentCache">Кэш содержимого репозитория</param>
        public PhiladelphusRepositoryService(
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryContentCache contentCache)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(contentCache);

            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _contentCache = contentCache;
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить содержимое репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="force">Признак принудительного получения из хранилища данных</param>
        /// <returns>Репозиторий с содержимым</returns>
        public PhiladelphusRepositoryModel GetShrubContent(PhiladelphusRepositoryModel repository, bool force = false)
        {
            ArgumentNullException.ThrowIfNull(repository);
            var shrub = repository.ContentShrub
                ?? throw new InvalidOperationException("Рабочий кустарник не инициализирован");

            repository.ContentShrub.ContentWorkingTrees.Clear();

            var sw = Stopwatch.StartNew();
            PhiladelphusRepositoryModel result;
            string sourceName;

            if (force)
            {
                result = GetShrubContentForce(repository);
                sourceName = "БД";
            }
            else
            {
                var cacheReadContext = new InfrastructureCacheReadContext();
                result = GetShrubContentFast(repository, cacheReadContext);
                sourceName = cacheReadContext.SourceName;
            }

            sw.Stop();

            SendContentLoadNotification("Содержимое репозитория", repository.Name, sourceName, sw.Elapsed);

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
            bool force = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(repository);

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return GetShrubContent(repository, force);
            }, cancellationToken);
        }

        /// <summary>
        /// Получить элементы рабочего дерева
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="force">Признак принудительного получения из хранилища данных</param>
        /// <returns>Дерево с содержимым</returns>
        public WorkingTreeModel GetWorkingTreeContent(WorkingTreeModel tree, bool force = false)
        {
            ArgumentNullException.ThrowIfNull(tree);

            var sw = Stopwatch.StartNew();
            WorkingTreeModel result;
            string sourceName;

            tree.ContentRoot = null;

            if (force)
            {
                result = GetWorkingTreeContentForce(tree);
                sourceName = "БД";
            }
            else
            {
                var cacheReadContext = new InfrastructureCacheReadContext();
                result = GetWorkingTreeContentFast(tree, cacheReadContext);
                sourceName = cacheReadContext.SourceName;
            }

            sw.Stop();

            SendContentLoadNotification("Содержимое рабочего дерева", tree.Name, sourceName, sw.Elapsed);

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
            bool force = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tree);

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return GetWorkingTreeContent(tree, force);
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
            var dbRoot = dbRoots.First();
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
