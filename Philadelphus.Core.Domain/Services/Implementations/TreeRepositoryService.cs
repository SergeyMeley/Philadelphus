using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System.Xml.Linq;
using System.Xml.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public PhiladelphusRepositoryModel GetShrub(PhiladelphusRepositoryModel repository)
        {
            repository.ContentShrub.ContentTrees.Clear();

            // TODO: Добавить кэш

            var result = GetShrubFromDb(repository);

            InitSystemWorkingTree(repository.ContentShrub);

            return result;
        }

        /// <summary>
        /// Загрузить коллекцию элементов репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с элементами</returns>
        public PhiladelphusRepositoryModel GetShrubFromDb(PhiladelphusRepositoryModel repository)
        {
            repository.ContentShrub.ContentTrees.Clear();

            foreach (var dataStorage in repository?.DataStorages)
            {
                if (dataStorage.IsAvailable)
                {
                    var infrastructure = dataStorage.PhiladelphusRepositoryMembersInfrastructureRepository;
                    var uuids = repository.ContentShrub.ContentTreesUuids?.ToArray();
                    var dbRoots = infrastructure.SelectRoots(uuids);
                    var roots = dbRoots?.ToModelCollection(repository.DataStorages, new List<PhiladelphusRepositoryModel>() { repository });

                    foreach (var root in roots.Where(x => x.OwningRepository.Uuid == repository.Uuid))
                    {
                        var tree = new WorkingTreeModel(root.Uuid, root.DataStorage, root.DbEntity as TreeRoot, repository.ContentShrub);
                        tree.UnavailableNames.Add(tree.Name);
                        tree.ContentRoot = root;
                        tree.UnavailableNames.Add(root.Name);
                        repository.ContentShrub.ContentTrees.Add(tree);

                        SetModelState(tree.ContentRoot, State.SavedOrLoaded);

                        GetPersonalAttributes(tree.ContentRoot);
                        GetWorkingTree(tree.ContentRoot);
                    }
                }
            }

            InitSystemWorkingTree(repository.ContentShrub);

            return repository;
        }

        /// <summary>
        /// Получить рабочее дерево
        /// </summary>
        /// <param name="root">Корень</param>
        /// <returns>Корень с содержимым</returns>
        public TreeRootModel GetWorkingTree(TreeRootModel root)
        {
            root.ChildNodes.Clear();

            // TODO: Добавить кэш

            return GetWorkingTreeFromDb(root);
        }

        /// <summary>
        /// Получить рабочее дерево
        /// </summary>
        /// <param name="root">Корень</param>
        /// <returns>Корень с содержимым</returns>
        public TreeRootModel GetWorkingTreeFromDb(TreeRootModel root)
        {
            var tree = root.OwningWorkingTree;

            root.ChildNodes.Clear();

            if (tree.DataStorage.IsAvailable)
            {
                var infrastructure = tree.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository;
                var dbNodes = infrastructure.SelectNodes(new[] { tree.ContentRoot.Uuid });
                var nodes = dbNodes?.ToModelCollection(new List<IParentModel>() { root });
                var allNodes = new List<TreeNodeModel>();
                while (nodes.Count > 0)
                {
                    allNodes.AddRange(nodes);
                    nodes = dbNodes?.ToModelCollection(nodes);
                }
                var dbLeaves = infrastructure.SelectLeaves(new[] { tree.ContentRoot.Uuid });
                var allLeaves = dbLeaves?.ToModelCollection(allNodes);

                foreach (var item in allNodes)
                {
                    tree.UnavailableNames.Add(item.Name);
                }
                foreach (var item in allLeaves)
                {
                    tree.UnavailableNames.Add(item.Name);
                }

                GetTreeRootMembersAndContent(tree.ContentRoot, allNodes, allLeaves);
            }

            return tree.ContentRoot;
        }

        /// <summary>
        /// Получить содержимое корня
        /// </summary>
        /// <param name="root">Корень</param>
        /// <returns>Корень с содержимым</returns>
        public TreeRootModel GetTreeRootMembersAndContent(TreeRootModel root, IEnumerable<TreeNodeModel> allNodes, IEnumerable<TreeLeaveModel> allLeaves)
        {
            GetPersonalAttributes(root);

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
                    GetTreeNodeMembersAndContent(child, allNodes, allLeaves);
                }
            }

            return root;
        }

        /// <summary>
        /// Получить содержимое узла
        /// </summary>
        /// <param name="node">Узел</param>
        /// <returns>Узел с содержимым</returns>
        public TreeNodeModel GetTreeNodeMembersAndContent(TreeNodeModel node, IEnumerable<TreeNodeModel> allNodes, IEnumerable<TreeLeaveModel> allLeaves)
        {
            GetPersonalAttributes(node);

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
                    GetTreeNodeMembersAndContent(child, allNodes, allLeaves);
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
                    GetTreeLeaveContent(child);
                }
            }

            return node;
        }

        /// <summary>
        /// Получить содержимое листа
        /// </summary>
        /// <param name="leave">Лист</param>
        /// <returns>Лист с содержимым</returns>
        public TreeLeaveModel GetTreeLeaveContent(TreeLeaveModel leave)
        {
            GetPersonalAttributes(leave);
            return leave;
        }

        /// <summary>
        /// Получить собственные атрибуты
        /// </summary>
        /// <param name="attributeOwner">Владелец атрибутов</param>
        /// <returns>Коллекция атрибутов</returns>
        public IEnumerable<ElementAttributeModel> GetPersonalAttributes(IAttributeOwnerModel attributeOwner)
        {
            attributeOwner.ClearAttributes();

            if (attributeOwner is IShrubMemberModel me)
            {
                var infrastructure = me.DataStorage.PhiladelphusRepositoryMembersInfrastructureRepository;
                var dbEntities = infrastructure.SelectAttributes().Where(x => x.OwnerUuid == attributeOwner.Uuid);
                var attributes = dbEntities?.ToModelCollection(new List<IAttributeOwnerModel>() { attributeOwner });

                if (attributes != null)
                {
                    foreach (var attribute in attributes)
                    {
                        SetModelState(attribute, State.SavedOrLoaded);
                        attributeOwner.AddAttribute(attribute);
                    }

                    return attributes;
                }
            }
            return null;
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

            // Сохранение изменений
            long result = 0;
            var infrastructure = repository.OwnDataStorage.PhiladelphusRepositoriesInfrastructureRepository;
            var entity = repository.ToDbEntity();
            var storages = repository.DataStorages;

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
                result += SaveChanges(repository.ContentShrub.ContentTrees.Select(x => x.ContentRoot), saveMode);
            }

            // Возвращение обновленной информации
            // Не переносить, ломается логика
            repository = infrastructure.SelectRepositories(new Guid[] { repository.Uuid }).First().ToModel(storages);  
            repository = GetShrub(repository);      // Не переносить, ломается логика

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
            foreach (var storage in treeRoots.Select(x => x.DataStorage).Distinct())
            {
                List<TreeRoot> dbCollection;
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.InsertRoots(dbCollection);
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.UpdateRoots(dbCollection);
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.DeleteRoots(dbCollection);
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
                result += SaveContentChanges(treeRoots.SelectMany(x => x.PersonalAttributes));
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
            foreach (var storage in treeNodes.Select(x => x.DataStorage).Distinct())
            {
                List<TreeNode> dbCollection;
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.InsertNodes(dbCollection);
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.UpdateNodes(dbCollection);
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.DeleteNodes(dbCollection);
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
                result += SaveContentChanges(treeNodes.SelectMany(x => x.PersonalAttributes).Where(x => treeNodes.Any(y => y.Uuid == x.Owner.Uuid)));
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
            foreach (var storage in treeLeaves.Select(x => x.DataStorage).Distinct())
            {
                List<TreeLeave> dbCollection;
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.InsertLeaves(dbCollection);
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.UpdateLeaves(dbCollection);
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.DeleteLeaves(dbCollection);
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
                result += SaveContentChanges(treeLeaves.SelectMany(x => x.PersonalAttributes));
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
            foreach (var storage in elementAttributes.Select(x => x.DataStorage).Distinct())
            {
                List<ElementAttribute> dbCollection;
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.InsertAttributes(dbCollection);
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.UpdateAttributes(dbCollection);
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.PhiladelphusRepositoryMembersInfrastructureRepository.DeleteAttributes(dbCollection);
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
                var dbRoot = new TreeRoot();
                var tree = new WorkingTreeModel(Guid.NewGuid(), dataStorage, dbRoot, parentElement.ContentShrub);
                var root = new TreeRootModel(tree.Uuid, tree, dbRoot);
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
                _notificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
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
                _notificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
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
                    _notificationService.SendNotification("Лист можно добавить только в узел.", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
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
                _notificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
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

                // Наследование атрибута дочерним узлам
                // Пытаюсь переделать push-модель на pull-модель, т.к. владельцы атрибутов сами высчитывают свой список атрибутов при обращении к нему
                //if (this is IParentModel p)
                //{
                //    foreach (var child in p.Childs)
                //    {
                //        if (child.Value is IAttributeOwnerModel ao)
                //        {
                //            CreateElementAttribute(ao, visibility);
                //        }
                //    }
                //}

                //if (owner is IMainEntityWritableModel o)
                //{
                //    SetModelState(o, State.SavedOrLoaded);
                //}

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания атрибута.", ex);
                _notificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
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
            var existTree = shrub.ContentTrees.SingleOrDefault(x => x.Uuid == Guid.Parse("00000000-0000-0000-0000-0000002018ee"));
            if (existTree != null)
                return false;

            // TODO: ех. долг #61187115
            var newUuid = Guid.Parse("00000000-0000-0000-0000-0000002018ee");

            var tree = new WorkingTreeModel(
                uuid: newUuid,
                dataStorage: shrub.DataStorage,
                dbEntity: new TreeRoot(),
                owner: shrub);

            var root = new TreeRootModel(
                uuid: newUuid,
                owner: tree,
                dbEntity: tree.DbEntity)
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
