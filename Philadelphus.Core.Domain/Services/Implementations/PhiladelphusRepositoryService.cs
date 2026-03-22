using AutoMapper;
using Microsoft.Extensions.Logging;
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
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Основной сервис для работы с репозиторием и его элементами
    /// </summary>
    public class PhiladelphusRepositoryService : IPhiladelphusRepositoryService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger<PhiladelphusRepositoryService> _logger;
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
            ILogger<PhiladelphusRepositoryService> logger,
            INotificationService notificationService)
        {
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
        /// <returns>Репозиторий с содержимым</returns>
        public PhiladelphusRepositoryModel GetShrubContent(PhiladelphusRepositoryModel repository)
        {
            //var oldShrub = repository.ContentShrub;
            //repository.ContentShrub = new ShrubModel(oldShrub.Uuid, oldShrub.DbEntity as PhiladelphusRepository, repository);
            repository.ContentShrub.ContentWorkingTrees.Clear();

            // TODO: Добавить кэш

            var result = GetShrubContentFromDb(repository);

            InitSystemWorkingTree(repository.ContentShrub);

            return result;
        }

        /// <summary>
        /// Загрузить коллекцию элементов репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с элементами</returns>
        public PhiladelphusRepositoryModel GetShrubContentFromDb(PhiladelphusRepositoryModel repository)
        {
            repository.ContentShrub.ContentWorkingTrees.Clear();

            foreach (var dataStorage in repository?.DataStorages)
            {
                if (dataStorage.IsAvailable)
                {
                    var infrastructure = dataStorage.ShrubMembersInfrastructureRepository;
                    var treesUuids = repository.ContentShrub.ContentWorkingTreesUuids?.ToArray();
                    var dbTrees = infrastructure.SelectTrees(treesUuids);
                    var trees = _mapper.MapWorkingTrees(dbTrees, repository.DataStorages, repository.ContentShrub);

                    foreach (var tree in trees?.Where(x => x.OwningRepository.Uuid == repository.Uuid))
                    {
                        tree.UnavailableNames.Add(tree.Name);

                        repository.ContentShrub.ContentWorkingTrees.Add(tree);

                        GetWorkingTreeContentFromDbWithoutAttributes(tree);

                        SetModelState(tree, State.SavedOrLoaded);
                    }

                    // Получение атрибутов всех элементов деревьев из БД
                    foreach (var tree in repository.ContentShrub.ContentWorkingTrees)
                    {
                        GetWorkingTreeContentFromDb(tree);
                    }
                }
            }

            InitSystemWorkingTree(repository.ContentShrub);

            SetModelState(repository, State.SavedOrLoaded);

            return repository;
        }

        /// <summary>
        /// Получить элементы рабочего дерева
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <returns>Дерево с содержимым</returns>
        public WorkingTreeModel GetWorkingTreeContent(WorkingTreeModel tree)
        {
            tree.ContentRoot = null;

            // TODO: Добавить кэш

            return GetWorkingTreeContentFromDb(tree);
        }

        /// <summary>
        /// Получить элементы рабочего дерева
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <returns>Дерево с содержимым</returns>
        public WorkingTreeModel GetWorkingTreeContentFromDb(WorkingTreeModel tree)
        {
            // Получение дерева без атрибутов
            tree.ContentRoot = null;
            GetWorkingTreeContentFromDbWithoutAttributes(tree);

            // Получение полных списков типов данных (узлов) и значений (листов) атрибутов со всего кустарника
            var allShrubNodes = tree.OwningShrub.ContentWorkingTrees.SelectMany(x => x.GetAllNodesRecursive() ?? new List<TreeNodeModel>())?.ToList();
            var allShrubLeaves = tree.OwningShrub.ContentWorkingTrees.SelectMany(x => x.GetAllLeavesRecursive() ?? new List<TreeLeaveModel>())?.ToList();

            // Формирование списка владельцев атрибутов
            var owners = new List<IAttributeOwnerModel>();
            owners.Add(tree);
            owners.Add(tree.ContentRoot);
            owners.AddRange(tree.GetAllNodesRecursive());
            owners.AddRange(tree.GetAllLeavesRecursive());

            // Получение атрибутов всех элементов дерева из БД
            var dbAttributes = tree.OwnDataStorage.ShrubMembersInfrastructureRepository.SelectAttributes(new[] { tree.Uuid });
            var allAttributes = _mapper.MapAttributes(dbAttributes, owners, allShrubNodes, allShrubLeaves, tree);

            // Распределение атрибутов по владельцам
            foreach (var item in owners)
            {
                DistributeAttributes(item, allAttributes, allShrubNodes, allShrubLeaves);
            }

            return tree;
        }

        private WorkingTreeModel GetWorkingTreeContentFromDbWithoutAttributes(WorkingTreeModel tree)
        {
            tree.ContentRoot = null;

            if (tree.DataStorage.IsAvailable)
            {
                var infrastructure = tree.DataStorage.ShrubMembersInfrastructureRepository;

                var dbRoot = infrastructure.SelectRoots(new Guid[] { tree.Uuid }).First();
                var root = _mapper.MapTreeRoot(dbRoot, tree);

                if (root != null)
                {
                    tree.ContentRoot = root;
                    SetModelState(root, State.SavedOrLoaded);

                    // Получение из БД всех элементов дерева
                    var dbNodes = infrastructure.SelectNodes(new[] { tree.Uuid });
                    var nodes = _mapper.MapTreeNodes(dbNodes, new[] { tree.ContentRoot }, tree.ContentRoot.OwningWorkingTree);
                    var allNodes = new List<TreeNodeModel>();
                    while (nodes.Count() > 0)
                    {
                        allNodes.AddRange(nodes);
                        nodes = _mapper.MapTreeNodes(dbNodes, nodes, tree.ContentRoot.OwningWorkingTree);;
                    }
                    var dbLeaves = infrastructure.SelectLeaves(new[] { tree.Uuid });
                    var allLeaves = _mapper.MapTreeLeaves(dbLeaves, allNodes, tree);

                    // Регистрация имен
                    tree.UnavailableNames.Add(root.Name);
                    foreach (var item in allNodes)
                    {
                        tree.UnavailableNames.Add(item.Name);
                    }
                    foreach (var item in allLeaves)
                    {
                        tree.UnavailableNames.Add(item.Name);
                    }

                    // Распределение дочерних элементов по родителям
                    DistributeTreeRootDescendants(tree.ContentRoot, allNodes, allLeaves);
                    foreach (var item in allNodes)
                    {
                        DistributeTreeNodeDescendants(item, allNodes, allLeaves);
                    }
                    foreach (var item in allLeaves)
                    {
                    }

                    // Распределение содержимого по владельцам
                    // Атрибуты здесь НЕ ДОБАВЛЯЕМ, вызываем ОТДЕЛЬНЫЙ МЕТОД из обертки
                }
            }

            return tree;
        }

        private TreeRootModel DistributeTreeRootDescendants(TreeRootModel root, IEnumerable<TreeNodeModel> allNodes, IEnumerable<TreeLeaveModel> allLeaves)
        {
            root.ChildNodes.Clear();

            var childNodes = allNodes.Where(x => x.Parent.Uuid == root.Uuid);

            if (childNodes != null)
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                root.ChildNodes.AddRange(childNodes);

                foreach (var child in childNodes)
                {
                    DistributeTreeNodeDescendants(child, allNodes, allLeaves);
                }
            }

            return root;
        }

        private TreeNodeModel DistributeTreeNodeDescendants(TreeNodeModel node, IEnumerable<TreeNodeModel> allNodes, IEnumerable<TreeLeaveModel> allLeaves)
        {
            node.ChildNodes.Clear();
            node.ChildLeaves.Clear();

            var childNodes = allNodes.Where(x => x.Parent.Uuid == node.Uuid);

            if (childNodes != null)
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                node.ChildNodes.AddRange(childNodes);

                foreach (var child in childNodes)
                {
                    DistributeTreeNodeDescendants(child, allNodes, allLeaves);
                }
            }

            var childLeaves = allLeaves.Where(x => x.Parent.Uuid == node.Uuid);

            if (childLeaves != null)
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
                SetModelState(attribute, State.SavedOrLoaded);
                attributeOwner.AddAttribute(attribute);
            }

            return attributeOwner.Attributes;
        }

        #endregion

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

            // Преобразование в сущность БД
            long result = 0;
            var infrastructure = repository.OwnDataStorage.PhiladelphusRepositoriesInfrastructureRepository;
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
                    break;
            }

            // Возвращение данных, генерируемых инфраструктурой
            repository.AuditInfo = _mapper.Map<AuditInfoModel>(entity.AuditInfo);

            // Актуализация статуса
            SetModelState(repository, State.SavedOrLoaded); // TODO: Удалить?

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

            // Возвращение обновленной информации
            // Не переносить, ломается логика
            //repository = infrastructure.SelectRepositories(new Guid[] { repository.Uuid }).First().ToModel(storage);  
            //repository = GetShrub(repository);      // Не переносить, ломается логика

            return result;
        }

        /// <summary>
        /// Сохранить изменения (корни)
        /// </summary>
        /// <param name="treeRoots">Коллекция корней</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<WorkingTreeModel> workingTrees, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (workingTrees == null || workingTrees.Count() == 0)
                return 0;

            // Сохранение изменений
            long result = 0;
            foreach (var infrastructure in workingTrees.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = workingTrees.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection, 
                    State.Initialized,
                    dbCollection => result += infrastructure.InsertTrees(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.Changed,
                    dbCollection => result += infrastructure.UpdateTrees(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<WorkingTreeModel, WorkingTree>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => result += infrastructure.SoftDeleteTrees(dbCollection),
                    ref result);
            }

            // Актуализация статуса
            foreach (var workingTree in workingTrees)
            {
                SetModelState(workingTree, State.SavedOrLoaded);
            }

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

            // Сохранение изменений
            long result = 0;
            foreach (var infrastructure in treeRoots.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeRoots.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => result += infrastructure.InsertRoots(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.Changed,
                    dbCollection => result += infrastructure.UpdateRoots(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeRootModel, TreeRoot>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => result += infrastructure.SoftDeleteRoots(dbCollection),
                    ref result);
            }

            // Актуализация статуса
            foreach (var treeRoot in treeRoots)
            {
                SetModelState(treeRoot, State.SavedOrLoaded);
            }

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

            // Сохранение изменений
            long result = 0;
            foreach (var infrastructure in treeNodes.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeNodes.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => result += infrastructure.InsertNodes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.Changed,
                    dbCollection => result += infrastructure.UpdateNodes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeNodeModel, TreeNode>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => result += infrastructure.SoftDeleteNodes(dbCollection),
                    ref result);
            }

            // Актуализация статуса
            foreach (var treeNode in treeNodes)
            {
                SetModelState(treeNode, State.SavedOrLoaded);
            }

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

            // Сохранение изменений
            long result = 0;
            foreach (var infrastructure in treeLeaves.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = treeLeaves.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => result += infrastructure.InsertLeaves(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.Changed,
                    dbCollection => result += infrastructure.UpdateLeaves(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<TreeLeaveModel, TreeLeave>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => result += infrastructure.SoftDeleteLeaves(dbCollection),
                    ref result);
            }

            // Актуализация статуса
            foreach (var treeLeave in treeLeaves)
            {
                SetModelState(treeLeave, State.SavedOrLoaded);
            }

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

            // Сохранение изменений
            long result = 0;
            foreach (var infrastructure in elementAttributes.Select(x => x.DataStorage).Distinct().Select(x => x.ShrubMembersInfrastructureRepository))
            {
                var fullCollection = elementAttributes.Where(x => x.DataStorage.ShrubMembersInfrastructureRepository == infrastructure).ToList();

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.Initialized,
                    dbCollection => result += infrastructure.InsertAttributes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.Changed,
                    dbCollection => result += infrastructure.UpdateAttributes(dbCollection),
                    ref result);

                SaveAndReturnAuditInfo<ElementAttributeModel, ElementAttribute>(
                    fullCollection,
                    State.ForSoftDelete,
                    dbCollection => result += infrastructure.SoftDeleteAttributes(dbCollection),
                    ref result);
            }

            // Актуализация статуса
            foreach (var elementAttribute in elementAttributes)
            {
                SetModelState(elementAttribute, State.SavedOrLoaded);
            }

            return result;
        }

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать корень и добавить родителю
        /// </summary>
        /// <param name="parentElement">Родитель</param>
        /// <param name="dataStorage">Хранилище</param>
        /// <returns>Корень</returns>
        public TreeRootModel CreateTreeRoot(PhiladelphusRepositoryModel parentElement, IDataStorageModel dataStorage)
        {
            try
            {
                // TODO: Вынести в отдельный метод
                var tree = new WorkingTreeModel(Guid.NewGuid(), dataStorage, parentElement.ContentShrub);
                var root = new TreeRootModel(tree.Uuid, tree);
                tree.ContentRoot = root;

                var result = tree.ContentRoot;

                parentElement.ContentShrub.ContentWorkingTrees.Add(tree);
                parentElement.ContentShrub.ContentWorkingTreesUuids.Add(tree.Uuid);
                SetModelState(parentElement, State.Changed);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания корня.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>($"Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }
        /// <summary>
        /// Создать узел и добавить родителю
        /// </summary>
        /// <param name="parentElement">Родитель</param>
        /// <returns>Узел</returns>
        public TreeNodeModel CreateTreeNode(IParentModel parentElement)
        {
            try
            {
                var result = new TreeNodeModel(Guid.NewGuid(), parentElement, (parentElement as IWorkingTreeMemberModel)?.OwningWorkingTree);
                // Переработал на pull-модель
                //if (parentElement is IAttributeOwnerModel)
                //    result.ParentElementAttributes = ((IAttributeOwnerModel)parentElement).Attributes;

                if (parentElement is TreeNodeModel node)
                    node.ChildNodes.Add(result);
                if (parentElement is TreeRootModel root)
                    root.ChildNodes.Add(result);

                SetModelState(result, State.Initialized);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания узла.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>($"Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }
        /// <summary>
        /// Создать лист и добавить родителю
        /// </summary>
        /// <param name="parentElement">Родитель</param>
        /// <returns>Лист</returns>
        public TreeLeaveModel CreateTreeLeave(TreeNodeModel parentElement)
        {
            try
            {
                if (parentElement is IPhiladelphusRepositoryMemberModel == false || parentElement is TreeNodeModel == false)
                {
                    _notificationService.SendTextMessage<PhiladelphusRepositoryService>("Лист можно добавить только в узел.");
                    return null;
                }
                else
                {
                    TreeLeaveModel result = null;
                    if (parentElement is SystemBaseTreeNodeModel sbn)
                    {
                        result = new SystemBaseTreeLeaveModel(Guid.NewGuid(), sbn, sbn.OwningWorkingTree, sbn.SystemBaseType);
                    }
                    else
                    {
                        result = new TreeLeaveModel(Guid.NewGuid(), parentElement, parentElement.OwningWorkingTree);
                    }
                    // Переработал на pull-модель
                    //result.ParentElementAttributes = parentElement.Attributes;
                    parentElement.ChildLeaves.Add(result);
                    //parentElement.State = State.Changed;
                    SetModelState(result, State.Initialized);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания листа.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>($"Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Создать атрибут и добавить владельцу
        /// </summary>
        /// <param name="owner">Владелец</param>
        /// <returns>Атрибут</returns>
        public ElementAttributeModel CreateElementAttribute(IAttributeOwnerModel owner)
        {
            try
            {
                var uuid = Guid.NewGuid();

                if (owner is WorkingTreeMemberBaseModel wtm)
                {
                    var result = new ElementAttributeModel(uuid, owner, uuid, owner, wtm.OwningWorkingTree)
                    {
                        Visibility = /*visibility*/ VisibilityScope.Public,
                        Override = OverrideType.Virtual
                    };

                    SetModelState(result, State.Initialized);

                    owner.AddAttribute(result);

                    return result;
                }
                _logger.LogWarning("Попытка добавления атрибута элементу, НЕ являющумяся участником рабочего дерева.");
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>($"Атрибут можно добавить только участнику рабочего дерева.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания атрибута.", ex);
                _notificationService.SendTextMessage<PhiladelphusRepositoryService>($"Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}");
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
                if (element == null)
                    return false;
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

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region [ Temp ]



        #endregion

        #region [ Private methods ]

        /// <summary>
        /// Изменить статус элемента
        /// </summary>
        /// <param name="model">Элемент</param>
        /// <param name="newState">Новый статус</param>
        private void SetModelState(IMainEntityWritableModel model, State newState)
        {
            model.SetState(newState);
        }


        /// <summary>
        /// Инициализировать системное рабочее дерево базовых типов
        /// </summary>
        /// <param name="shrub"></param>
        /// <returns></returns>
        private bool InitSystemWorkingTree(ShrubModel shrub)
        {
            var existTree = shrub.ContentWorkingTrees.SingleOrDefault(x => x.Uuid == WorkingTreeModel.SystemBaseGuid);
            if (existTree != null)
                return false;

            var dbExistTrees = shrub.DataStorage.ShrubMembersInfrastructureRepository
                .SelectTrees(new Guid[] { WorkingTreeModel.SystemBaseGuid });
            existTree = _mapper.MapWorkingTrees(dbExistTrees, shrub.OwningRepository.DataStorages, shrub).SingleOrDefault();

            if (existTree != null)
            {
                shrub.ContentWorkingTrees.Add(existTree);
                shrub.ContentWorkingTreesUuids.Add(existTree.Uuid);
                return true;
            }

            // TODO: ех. долг #61187115

            var tree = new WorkingTreeModel(
                uuid: WorkingTreeModel.SystemBaseGuid,
                dataStorage: shrub.DataStorage,
                owner: shrub);

            var root = new TreeRootModel(
                uuid: TreeRootModel.SystemBaseGuid,
                owner: tree)
            {
                Name = "Базовые типы данных"
            };

            tree.ContentRoot = root;

            var obj = new SystemBaseTreeNodeModel(root, tree, SystemBaseType.OBJECT);
            root.ChildNodes.Add(obj);

            var str = new SystemBaseTreeNodeModel(obj, tree, SystemBaseType.STRING);
            obj.ChildNodes.Add(str);

            var num = new SystemBaseTreeNodeModel(obj, tree, SystemBaseType.NUMERIC);
            obj.ChildNodes.Add(num);

            var integer = new SystemBaseTreeNodeModel(num, tree, SystemBaseType.INTEGER);
            num.ChildNodes.Add(integer);

            var flt = new SystemBaseTreeNodeModel(num, tree, SystemBaseType.FLOAT);
            num.ChildNodes.Add(flt);

            shrub.ContentWorkingTrees.Add(tree);
            shrub.ContentWorkingTreesUuids.Add(tree.Uuid);
            return true;
        }

        private void SaveAndReturnAuditInfo<TMainEntityModel, TMainEntity>(
            IEnumerable<TMainEntityModel> fullCollection,
            State state,
            Func<IEnumerable<TMainEntity>, long> persister,
            ref long result)
            where TMainEntityModel : IMainEntityModel
            where TMainEntity : IMainEntity
        {
            var collection = fullCollection.Where(x => x.State == state);
            var dbCollection = _mapper.Map<List<TMainEntity>>(collection);

            result += persister(dbCollection);

            foreach (var (src, dest) in collection.Zip(dbCollection, (src, dest) => (src, dest)))
            {
                src.AuditInfo = _mapper.Map<AuditInfoModel>(dest.AuditInfo);
            }
        }


        #endregion

    }
}
