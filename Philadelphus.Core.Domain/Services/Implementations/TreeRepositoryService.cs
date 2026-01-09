using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Основной сервис для работы с репозиторием и его элементами
    /// </summary>
    public class TreeRepositoryService : ITreeRepositoryService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger<TreeRepositoryService> _logger;
        private readonly INotificationService _notificationService;

        private static RepositoryMembersCollectionModel _mainEntityCollection = new RepositoryMembersCollectionModel();
        /// <summary>
        /// Коллекция элементов репозитория
        /// </summary>
        public static RepositoryMembersCollectionModel MainEntityCollection { get => _mainEntityCollection; }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Основной сервис для работы с репозиторием и его элементами
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Сервис логгирования</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        public TreeRepositoryService(
            IMapper mapper,
            ILogger<TreeRepositoryService> logger,
            INotificationService notificationService)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получение элемента репозитория по его UUID
        /// </summary>
        /// <param name="uuid">UUID элемента репозитория</param>
        /// <returns>Элемент репозитория</returns>
        public IMainEntity GetEntityFromCollection(Guid uuid)
        {
            return GetModelFromCollection(uuid).ToDbEntity();
        }
        /// <summary>
        /// Получение элемента репозитория (модель) по его UUID
        /// </summary>
        /// <param name="uuid">UUID элемента репозитория</param>
        /// <returns>Элемент репозитория (модель)</returns>
        public IMainEntityModel GetModelFromCollection(Guid uuid)
        {
            return _mainEntityCollection.FirstOrDefault(x => x.Uuid == uuid);
        }
        /// <summary>
        /// Загрузить коллекцию элементов репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с элементами</returns>
        public TreeRepositoryModel ForceLoadTreeRepositoryMembersCollection(TreeRepositoryModel repository)
        {
            foreach (var dataStorage in repository?.DataStorages)
            {
                var infrastructure = dataStorage.TreeRepositoryMembersInfrastructureRepository;
                if (infrastructure is ITreeRepositoriesMembersInfrastructureRepository
                    && dataStorage.IsAvailable)
                {
                    var dbRoots = infrastructure.SelectRoots(repository.ChildsUuids?.ToArray());
                    var roots = dbRoots?.ToModelCollection(repository.DataStorages, new List<TreeRepositoryModel>() { repository });
                    MainEntityCollection.DataTreeRoots.AddRange(roots);

                    var dbNodes = infrastructure.SelectNodes(repository.ChildsUuids?.ToArray());
                    var nodes = dbNodes?.ToModelCollection(MainEntityCollection.DataTreeRoots);
                    while (nodes.Count > 0)
                    {
                        MainEntityCollection.DataTreeNodes.AddRange(nodes);
                        nodes = dbNodes?.ToModelCollection(nodes);
                    }
                    
                    var dbLeaves = infrastructure.SelectLeaves(repository.ChildsUuids?.ToArray());
                    var leaves = dbLeaves?.ToModelCollection(MainEntityCollection.DataTreeNodes);
                    MainEntityCollection.DataTreeLeaves.AddRange(leaves);

                    var dbAttributes = infrastructure.SelectAttributes();
                    var owners = roots.Cast<IAttributeOwnerModel>()
                        .Concat(nodes.Cast<IAttributeOwnerModel>())
                        .Concat(leaves.Cast<IAttributeOwnerModel>());
                    var attributes = dbAttributes?.ToModelCollection(owners);
                    MainEntityCollection.ElementAttributes.AddRange(attributes);
                }
            }
            return repository;
        }
        /// <summary>
        /// Получить содержимое репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с содержимым</returns>
        public TreeRepositoryModel GetTreeRepositoryMembersAndContent(TreeRepositoryModel repository)
        {
            ForceLoadTreeRepositoryMembersCollection(repository);

            repository.Childs.Clear();

            var childRoots = MainEntityCollection.DataTreeRoots.Where(x => x.ParentRepository.Uuid == repository.Uuid);

            if (childRoots != null)
            {
                foreach (var child in childRoots)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                repository.Childs.AddRange(childRoots.Cast<IChildrenModel>().ToList());

                foreach (var child in childRoots)
                {
                    GetTreeRootMembersAndContent(child);
                }
            }
            return repository;
        }
        /// <summary>
        /// Получить содержимое корня
        /// </summary>
        /// <param name="root">Корень</param>
        /// <returns>Корень с содержимым</returns>
        public TreeRootModel GetTreeRootMembersAndContent(TreeRootModel root)
        {
            GetPersonalAttributes(root);

            root.Childs.Clear();

            var childNodes = MainEntityCollection.DataTreeNodes.Where(x => x.Parent.Uuid == root.Uuid);

            if (childNodes != null)
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                root.Childs.AddRange(childNodes.Cast<IChildrenModel>().ToList());

                foreach (var child in childNodes)
                {
                    GetTreeNodeMembersAndContent(child);
                }
            }

            return root;
        }
        /// <summary>
        /// Получить содержимое узла
        /// </summary>
        /// <param name="node">Узел</param>
        /// <returns>Узел с содержимым</returns>
        public TreeNodeModel GetTreeNodeMembersAndContent(TreeNodeModel node)
        {
            GetPersonalAttributes(node);

            node.Childs.Clear();

            var childNodes = MainEntityCollection.DataTreeNodes.Where(x => x.Parent.Uuid == node.Uuid);

            if (childNodes != null)
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                node.Childs.AddRange(childNodes.Cast<IChildrenModel>().ToList());

                foreach (var child in childNodes)
                {
                    GetTreeNodeMembersAndContent(child);
                }
            }

            var childLeaves = MainEntityCollection.DataTreeLeaves.Where(x => x.Parent.Uuid == node.Uuid);

            if (childLeaves != null)
            {
                foreach (var child in childLeaves)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                node.Childs.AddRange(childLeaves.Cast<IChildrenModel>().ToList());

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

            attributeOwner.PersonalAttributes.Clear();
            var attributes = MainEntityCollection.ElementAttributes.Where(x => x.Owner.Uuid == attributeOwner.Uuid);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    SetModelState(attribute, State.SavedOrLoaded);
                }
                attributeOwner.PersonalAttributes.AddRange(attributes);
                return attributes;
            }
            return null;
        }

        #endregion

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий)
        /// </summary>
        /// <param name="treeRepository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(TreeRepositoryModel treeRepository, SaveMode saveMode)
        {
            // Проверка исходных данных
            if (treeRepository == null)
                return 0;

            // Сохранение изменений
            long result = 0;
            switch (treeRepository.State)
            {
                case State.Initialized:
                    result += treeRepository.OwnDataStorage.TreeRepositoriesInfrastructureRepository.InsertRepository(treeRepository.ToDbEntity());
                    break;
                case State.Changed:
                    //var entity = _mapper.Map<TreeRepositoryModel, TreeRepository>(treeRepository);
                    var entity = treeRepository.ToDbEntity();
                    result += treeRepository.OwnDataStorage.TreeRepositoriesInfrastructureRepository.UpdateRepository(entity);
                    break;
                case State.ForSoftDelete:
                    result += treeRepository.OwnDataStorage.TreeRepositoriesInfrastructureRepository.DeleteRepository(treeRepository.ToDbEntity());
                    break;
                default:
                    break;
            }

            // Актуализация статуса
            treeRepository.State = State.SavedOrLoaded;

            // Сохранение содержимого
            if (saveMode == SaveMode.WithContent || 
                saveMode == SaveMode.WithContentAndMembers)
            {
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(treeRepository.ChildTreeRoots, saveMode);
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

            // Добавление элементов в коллекцию
            foreach (var treeRoot in treeRoots)
            {
                if (treeRoot.State == State.Initialized && _mainEntityCollection.DataTreeRoots.Any(x => x.Uuid == treeRoot.Uuid) == false)
                {
                    _mainEntityCollection.DataTreeRoots.Add(treeRoot);
                }
            }

            // Сохранение изменений
            long result = 0;
            foreach (var storage in treeRoots.Select(x => x.DataStorage).Distinct())
            {
                List<TreeRoot> dbCollection;
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.InsertRoots(dbCollection);
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.UpdateRoots(dbCollection);
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.DeleteRoots(dbCollection);
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
                result += SaveChanges(treeRoots.SelectMany(x => x.ChildTreeNodes), saveMode);
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

            // Добавление элементов в коллекцию
            foreach (var treeNode in treeNodes)
            {
                if (treeNode.State == State.Initialized && _mainEntityCollection.DataTreeRoots.Any(x => x.Uuid == treeNode.Uuid) == false)
                {
                    _mainEntityCollection.DataTreeNodes.Add(treeNode);
                }
            }

            // Сохранение изменений
            long result = 0;
            foreach (var storage in treeNodes.Select(x => x.DataStorage).Distinct())
            {
                List<TreeNode> dbCollection;
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.InsertNodes(dbCollection);
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.UpdateNodes(dbCollection);
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.DeleteNodes(dbCollection);
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
                result += SaveContentChanges(treeNodes.SelectMany(x => x.PersonalAttributes));
            }

            // Сохранение участников
            if (saveMode == SaveMode.WithContentAndMembers)
            {
                result += SaveChanges(treeNodes.SelectMany(x => x.ChildTreeNodes), saveMode);
                result += SaveChanges(treeNodes.SelectMany(x => x.ChildTreeLeaves), saveMode);
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

            // Добавление элементов в коллекцию
            foreach (var treeLeave in treeLeaves)
            {
                if (treeLeave.State == State.Initialized && _mainEntityCollection.DataTreeLeaves.Any(x => x.Uuid == treeLeave.Uuid) == false)
                {
                    _mainEntityCollection.DataTreeLeaves.Add(treeLeave);
                }
            }

            // Сохранение изменений
            long result = 0;
            foreach (var storage in treeLeaves.Select(x => x.DataStorage).Distinct())
            {
                List<TreeLeave> dbCollection;
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.InsertLeaves(dbCollection);
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.UpdateLeaves(dbCollection);
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.DeleteLeaves(dbCollection);
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

            // Добавление элементов в коллекцию
            //TODO: Вынести в отдельный метод AddInitialized
            foreach (var elementAttribute in elementAttributes)
            {
                if (elementAttribute.State == State.Initialized && _mainEntityCollection.ElementAttributes.Any(x => x.Uuid == elementAttribute.Uuid) == false)
                {
                    _mainEntityCollection.ElementAttributes.Add(elementAttribute);
                }
            }

            // Сохранение изменений
            long result = 0;
            foreach (var storage in elementAttributes.Select(x => x.DataStorage).Distinct())
            {
                List<ElementAttribute> dbCollection;
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.InsertAttributes(dbCollection);
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.UpdateAttributes(dbCollection);
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.TreeRepositoryMembersInfrastructureRepository.DeleteAttributes(dbCollection);
            }

            // Актуализация статуса
            foreach (var elementAttribute in elementAttributes)
            {
                SetModelState(elementAttribute, State.SavedOrLoaded);
            }


            //TODO: Вынести в отдельный метод RemoveDeleted
            //TODO: ПОЧИНИТЬ!!!
            foreach (var elementAttribute in elementAttributes)
            {
                if ((elementAttribute.State == State.ForHardDelete 
                    || elementAttribute.State == State.ForSoftDelete 
                    || elementAttribute.State == State.SoftDeleted) 
                    && _mainEntityCollection.Any(x => x.Uuid == elementAttribute.Uuid) == false)
                {
                    _mainEntityCollection.Remove(elementAttribute);
                }
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
        public TreeRootModel CreateTreeRoot(TreeRepositoryModel parentElement, IDataStorageModel dataStorage)
        {
            try
            {
                var result = new TreeRootModel(Guid.NewGuid(), parentElement, dataStorage, new TreeRoot());
                parentElement.ElementsCollection.Add(result);
                parentElement.Childs.Add(result);
                parentElement.State = State.Changed;
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
                var result = new TreeNodeModel(Guid.NewGuid(), parentElement, new TreeNode());
                if (parentElement is IAttributeOwnerModel)
                    result.ParentElementAttributes = ((IAttributeOwnerModel)parentElement).Attributes;
                result.ParentRepository.ElementsCollection.Add(result);
                //parentElement.State = State.Changed;
                //if (parentElement is IMainEntityWritableModel)
                //{
                //    SetModelState((IMainEntityWritableModel)parentElement, State.SavedOrLoaded);
                //}
                parentElement.Childs.Add(result);
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
                if (parentElement.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)) == false || parentElement.GetType() != typeof(TreeNodeModel))
                {
                    _notificationService.SendNotification("Лист можно добавить только в узел.", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
                    return null;
                }
                else
                {
                    var result = new TreeLeaveModel(Guid.NewGuid(), parentElement, new TreeLeave());
                    result.ParentElementAttributes = parentElement.Attributes;
                    result.ParentRepository.ElementsCollection.Add(result);
                    parentElement.Childs.Add(result);
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
                var result = new ElementAttributeModel(Guid.NewGuid(), owner, new ElementAttribute());
                owner.PersonalAttributes.Add(result);

                if (owner is IMainEntityWritableModel)
                {
                    SetModelState((IMainEntityWritableModel)owner, State.SavedOrLoaded);
                }
                
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
        public bool SoftDeleteRepositoryMember(IChildrenModel element)
        {
            try
            {
                //if (element == null)
                //    return false;
                
                //if (element.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)) && element.GetType().IsAssignableTo(typeof(TreeRepositoryMemberBaseModel)))
                //{
                //    ((List<TreeRepositoryMemberBaseModel>)((ITreeRepositoryMemberModel)element).ParentRepository.ElementsCollection).Remove((TreeRepositoryMemberBaseModel)element);
                //}
                ////
                //return true;

                if (element == null)
                    return false;
                long result = 0;
                if (element is IMainEntityWritableModel)
                {
                    SetModelState((IMainEntityWritableModel)element, State.ForSoftDelete);
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

        internal List<ElementAttributeModel> GetAttributesSample(IAttributeOwnerModel owner)
        {
            var result = new List<ElementAttributeModel>();

            for (int i = 0; i < 20; i++)
            {
                var entry = new ElementAttributeModel(Guid.NewGuid(), owner, null);
                owner.PersonalAttributes.Add(entry);
                result.Add(entry);
            }

            return result;
        }
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

        #endregion

    }
}
