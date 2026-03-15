using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
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
            repository.ContentShrub.ContentTrees.Clear();

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
            repository.ContentShrub.ContentTrees.Clear();

            foreach (var dataStorage in repository?.DataStorages)
            {
                if (dataStorage.IsAvailable)
                {
                    var infrastructure = dataStorage.PhiladelphusRepositoryMembersInfrastructureRepository;
                    var treesUuids = repository.ContentShrub.ContentTreesUuids?.ToArray();
                    var dbTrees = infrastructure.SelectTrees(treesUuids);
                    var trees = dbTrees.ToModelCollection(repository.DataStorages, repository);

                    foreach (var tree in trees?.Where(x => x.OwningRepository.Uuid == repository.Uuid))
                    {
                        tree.UnavailableNames.Add(tree.Name);

                        repository.ContentShrub.ContentTrees.Add(tree);

                        GetWorkingTreeContentFromDbWithoutAttributes(tree);

                        SetModelState(tree, State.SavedOrLoaded);
                    }

                    // Получение атрибутов всех элементов деревьев из БД
                    foreach (var tree in repository.ContentShrub.ContentTrees)
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
            var allShrubNodes = tree.OwningShrub.ContentTrees.SelectMany(x => x.GetAllNodesRecursive() ?? new List<TreeNodeModel>())?.ToList();
            var allShrubLeaves = tree.OwningShrub.ContentTrees.SelectMany(x => x.GetAllLeavesRecursive() ?? new List<TreeLeaveModel>())?.ToList();

            // Формирование списка владельцев атрибутов
            var owners = new List<IAttributeOwnerModel>();
            owners.Add(tree);
            owners.Add(tree.ContentRoot);
            var qwe = tree.GetAllNodesRecursive();
            owners.AddRange(tree.GetAllNodesRecursive());
            owners.AddRange(tree.GetAllLeavesRecursive());

            // Получение атрибутов всех элементов дерева из БД
            var dbAttributes = tree.OwnDataStorage.PhiladelphusRepositoryMembersInfrastructureRepository.SelectAttributes();
            var allAttributes = dbAttributes?.ToModelCollection(owners, allShrubNodes, allShrubLeaves);

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
                var infrastructure = tree.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository;

                var dbRoots = infrastructure.SelectRoots(new Guid[] { tree.Uuid });
                var roots = dbRoots.ToModelCollection(new List<WorkingTreeModel> { tree });
                var root = roots.SingleOrDefault(x => x.OwningWorkingTree.Uuid == tree.Uuid);

                if (root != null)
                {
                    tree.ContentRoot = root;
                    SetModelState(root, State.SavedOrLoaded);

                    // Получение из БД всех элементов дерева
                    var dbNodes = infrastructure.SelectNodes(new[] { tree.Uuid });
                    var nodes = dbNodes?.ToModelCollection(new List<IParentModel>() { tree.ContentRoot });
                    var allNodes = new List<TreeNodeModel>();
                    while (nodes.Count > 0)
                    {
                        allNodes.AddRange(nodes);
                        nodes = dbNodes?.ToModelCollection(nodes);
                    }
                    var dbLeaves = infrastructure.SelectLeaves(new[] { tree.Uuid });
                    var allLeaves = dbLeaves?.ToModelCollection(allNodes);

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
            var entity = repository.ToDbEntity();
            var storage = repository.OwnDataStorage;

             // Сохранение изменений
            switch (repository.State)
            {
                case State.Initialized:
                    result += infrastructure.InsertRepository(entity);
                    break;
                case State.Changed:
                    //var entity = _mapper.Map<PhiladelphusRepositoryModel, PhiladelphusRepository>(PhiladelphusRepository);
                    result += infrastructure.UpdateRepository(entity);
                    break;
                case State.ForSoftDelete:
                    result += infrastructure.DeleteRepository(entity);
                    break;
                default:
                    break;
            }

            // Возвращение данных, генерируемых инфраструктурой
            repository.AuditInfo = entity.AuditInfo.ToModel();

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
                result += SaveChanges(repository.ContentShrub.ContentTrees, saveMode);
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
            foreach (var infrastructure in workingTrees.Select(x => x.DataStorage).Distinct().Select(x => x.PhiladelphusRepositoryMembersInfrastructureRepository))
            {
                var collection = workingTrees.Where(x => x.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository == infrastructure).ToList();
                List<WorkingTree> dbCollection;
                dbCollection = collection.Where(x => x.State == State.Initialized).ToDbEntityCollection();
                result += infrastructure.InsertTrees(dbCollection);
                dbCollection = collection.Where(x => x.State == State.Changed).ToDbEntityCollection();
                result += infrastructure.UpdateTrees(dbCollection);
                dbCollection = collection.Where(x => x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += infrastructure.SoftDeleteTrees(dbCollection);
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
            foreach (var infrastructure in treeRoots.Select(x => x.DataStorage).Distinct().Select(x => x.PhiladelphusRepositoryMembersInfrastructureRepository))
            {
                var collection = treeRoots.Where(x => x.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository == infrastructure).ToList();
                List<TreeRoot> dbCollection;
                dbCollection = collection.Where(x => x.State == State.Initialized).ToDbEntityCollection();
                result += infrastructure.InsertRoots(dbCollection);
                dbCollection = collection.Where(x => x.State == State.Changed).ToDbEntityCollection();
                result += infrastructure.UpdateRoots(dbCollection);
                dbCollection = collection.Where(x => x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += infrastructure.SoftDeleteRoots(dbCollection);
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
            foreach (var infrastructure in treeNodes.Select(x => x.DataStorage).Distinct().Select(x => x.PhiladelphusRepositoryMembersInfrastructureRepository))
            {
                var collection = treeNodes.Where(x => x.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository == infrastructure).ToList();
                List<TreeNode> dbCollection;
                dbCollection = collection.Where(x => x.State == State.Initialized).ToDbEntityCollection();
                result += infrastructure.InsertNodes(dbCollection);
                dbCollection = collection.Where(x => x.State == State.Changed).ToDbEntityCollection();
                result += infrastructure.UpdateNodes(dbCollection);
                dbCollection = collection.Where(x => x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += infrastructure.SoftDeleteNodes(dbCollection);
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
            foreach (var infrastructure in treeLeaves.Select(x => x.DataStorage).Distinct().Select(x => x.PhiladelphusRepositoryMembersInfrastructureRepository))
            {
                var collection = treeLeaves.Where(x => x.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository == infrastructure).ToList();
                List<TreeLeave> dbCollection;
                dbCollection = collection.Where(x => x.State == State.Initialized).ToDbEntityCollection();
                result += infrastructure.InsertLeaves(dbCollection);
                dbCollection = collection.Where(x => x.State == State.Changed).ToDbEntityCollection();
                result += infrastructure.UpdateLeaves(dbCollection);
                dbCollection = collection.Where(x => x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += infrastructure.SoftDeleteLeaves(dbCollection);
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
            foreach (var infrastructure in elementAttributes.Select(x => x.DataStorage).Distinct().Select(x => x.PhiladelphusRepositoryMembersInfrastructureRepository))
            {
                var collection = elementAttributes.Where(x => x.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository == infrastructure).ToList();
                List<ElementAttribute> dbCollection;
                dbCollection = collection.Where(x => x.State == State.Initialized).ToDbEntityCollection();
                result += infrastructure.InsertAttributes(dbCollection);
                dbCollection = collection.Where(x => x.State == State.Changed).ToDbEntityCollection();
                result += infrastructure.UpdateAttributes(dbCollection);
                dbCollection = collection.Where(x => x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += infrastructure.SoftDeleteAttributes(dbCollection);
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
                var tree = new WorkingTreeModel(Guid.NewGuid(), dataStorage, new WorkingTree(), parentElement.ContentShrub);
                var root = new TreeRootModel(tree.Uuid, tree, new TreeRoot());
                tree.ContentRoot = root;

                var result = tree.ContentRoot;

                parentElement.ContentShrub.ContentTrees.Add(tree);
                parentElement.ContentShrub.ContentTreesUuids.Add(tree.Uuid);
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
                var result = new TreeNodeModel(Guid.NewGuid(), parentElement, (parentElement as IWorkingTreeMemberModel)?.OwningWorkingTree, new TreeNode());
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
                        result = new TreeLeaveModel(Guid.NewGuid(), parentElement, parentElement.OwningWorkingTree, new TreeLeave());
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
                var result = new ElementAttributeModel(uuid, owner, uuid, owner, new ElementAttribute())
                {
                    Visibility = /*visibility*/ VisibilityScope.Public,
                    Override = OverrideType.Virtual
                };

                SetModelState(result, State.Initialized);

                owner.AddAttribute(result);


                return result;
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
        public bool SoftDeleteRepositoryMember(IContentModel element)
        {
            try
            {
                //if (element == null)
                //    return false;
                
                //if (element.GetType().IsAssignableTo(typeof(IPhiladelphusRepositoryMemberModel)) && element.GetType().IsAssignableTo(typeof(PhiladelphusRepositoryMemberBaseModel)))
                //{
                //    ((List<PhiladelphusRepositoryMemberBaseModel>)((IPhiladelphusRepositoryMemberModel)element).ParentRepository.ElementsCollection).Remove((PhiladelphusRepositoryMemberBaseModel)element);
                //}
                ////
                //return true;

                if (element == null)
                    return false;
                long result = 0;
                if (element is IMainEntityWritableModel me)
                {
                    SetModelState(me, State.ForSoftDelete);
                }
                if (element is WorkingTreeModel wt)
                {
                    wt.OwningShrub.ContentTreesUuids.Remove(wt.Uuid);
                    SetModelState(wt.OwningShrub, State.Changed);
                    SetModelState(wt.ContentRoot, State.ForSoftDelete);
                }
                if (element is TreeRootModel r)
                {
                    SetModelState(r.OwningWorkingTree, State.ForSoftDelete);
                    r.OwningShrub.ContentTreesUuids.Remove(r.Uuid);
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
            var existTree = shrub.ContentTrees.SingleOrDefault(x => x.Uuid == WorkingTreeModel.SystemBaseGuid);
            if (existTree != null)
                return false;
            var dbExistTree = shrub.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository
                .SelectTrees(new Guid[] { WorkingTreeModel.SystemBaseGuid })?
                .ToModelCollection(new List<IDataStorageModel>() { shrub.DataStorage }, shrub.OwningRepository)?
                .SingleOrDefault();
            if (dbExistTree != null)
            {
                shrub.ContentTrees.Add(dbExistTree);
                shrub.ContentTreesUuids.Add(dbExistTree.Uuid);
                return true;
            }

            // TODO: ех. долг #61187115

            var tree = new WorkingTreeModel(
                uuid: WorkingTreeModel.SystemBaseGuid,
                dataStorage: shrub.DataStorage,
                dbEntity: new WorkingTree(),
                owner: shrub);

            var root = new TreeRootModel(
                uuid: TreeRootModel.SystemBaseGuid,
                owner: tree,
                dbEntity: new TreeRoot())
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

            shrub.ContentTrees.Add(tree);
            shrub.ContentTreesUuids.Add(tree.Uuid);
            return true;
        }

        #endregion

    }
}
