using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;

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
        /// <param name="guid">UUID элемента репозитория</param>
        /// <returns>Элемент репозитория</returns>
        public IMainEntity GetEntityFromCollection(Guid guid)
        {
            return GetModelFromCollection(guid).ToDbEntity();
        }
        /// <summary>
        /// Получение элемента репозитория (модель) по его UUID
        /// </summary>
        /// <param name="guid">UUID элемента репозитория</param>
        /// <returns>Элемент репозитория (модель)</returns>
        public IMainEntityModel GetModelFromCollection(Guid guid)
        {
            return _mainEntityCollection.FirstOrDefault(x => x.Guid == guid);
        }
        /// <summary>
        /// Загрузить коллекцию элементов репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с элементами</returns>
        public TreeRepositoryModel LoadMainEntityCollection(TreeRepositoryModel repository)
        {
            foreach (var dataStorage in repository?.DataStorages)
            {
                var infrastructure = dataStorage.MainEntitiesInfrastructureRepository;
                if (infrastructure is IMainEntitiesInfrastructureRepository
                    && dataStorage.IsAvailable)
                {
                    var dbRoots = infrastructure.SelectRoots(repository.ChildsGuids?.ToArray());
                    var roots = dbRoots?.ToModelCollection(repository.DataStorages, new List<TreeRepositoryModel>() { repository });
                    MainEntityCollection.DataTreeRoots.AddRange(roots);

                    var dbNodes = infrastructure.SelectNodes(repository.ChildsGuids?.ToArray());
                    var nodes = dbNodes?.ToModelCollection(MainEntityCollection.DataTreeRoots);
                    while (nodes.Count > 0)
                    {
                        MainEntityCollection.DataTreeNodes.AddRange(nodes);
                        nodes = dbNodes?.ToModelCollection(nodes);
                    }
                    
                    var dbLeaves = infrastructure.SelectLeaves(repository.ChildsGuids?.ToArray());
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
        public TreeRepositoryModel GetRepositoryContent(TreeRepositoryModel repository)
        {
            LoadMainEntityCollection(repository);

            repository.Childs.Clear();

            var childRoots = MainEntityCollection.DataTreeRoots.Where(x => x.ParentRepository.Guid == repository.Guid);

            if (childRoots != null)
            {
                foreach (var child in childRoots)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                repository.Childs.AddRange(childRoots.Cast<IChildrenModel>().ToList());

                foreach (var child in childRoots)
                {
                    GetRootContent(child);
                }
            }
            return repository;
        }
        /// <summary>
        /// Получить содержимое корня
        /// </summary>
        /// <param name="root">Корень</param>
        /// <returns>Корень с содержимым</returns>
        public TreeRootModel GetRootContent(TreeRootModel root)
        {
            GetPersonalAttributes(root);

            root.Childs.Clear();

            var childNodes = MainEntityCollection.DataTreeNodes.Where(x => x.Parent.Guid == root.Guid);

            if (childNodes != null)
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                root.Childs.AddRange(childNodes.Cast<IChildrenModel>().ToList());

                foreach (var child in childNodes)
                {
                    GetNodeContent(child);
                }
            }

            return root;
        }
        /// <summary>
        /// Получить содержимое узла
        /// </summary>
        /// <param name="node">Узел</param>
        /// <returns>Узел с содержимым</returns>
        public TreeNodeModel GetNodeContent(TreeNodeModel node)
        {
            GetPersonalAttributes(node);

            node.Childs.Clear();

            var childNodes = MainEntityCollection.DataTreeNodes.Where(x => x.Parent.Guid == node.Guid);

            if (childNodes != null)
            {
                foreach (var child in childNodes)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                node.Childs.AddRange(childNodes.Cast<IChildrenModel>().ToList());

                foreach (var child in childNodes)
                {
                    GetNodeContent(child);
                }
            }

            var childLeaves = MainEntityCollection.DataTreeLeaves.Where(x => x.Parent.Guid == node.Guid);

            if (childLeaves != null)
            {
                foreach (var child in childLeaves)
                {
                    SetModelState(child, State.SavedOrLoaded);
                }
                node.Childs.AddRange(childLeaves.Cast<IChildrenModel>().ToList());

                foreach (var child in childLeaves)
                {
                    GetLeaveContent(child);
                }
            }

            return node;
        }
        /// <summary>
        /// Получить содержимое листа
        /// </summary>
        /// <param name="leave">Лист</param>
        /// <returns>Лист с содержимым</returns>
        public TreeLeaveModel GetLeaveContent(TreeLeaveModel leave)
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
            var attributes = MainEntityCollection.ElementAttributes.Where(x => x.Owner.Guid == attributeOwner.Guid);
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
        /// Сохранить изменения
        /// </summary>
        /// <param name="treeRepository">Репозиторий для сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(TreeRepositoryModel treeRepository)
        {
            if (treeRepository == null)
                return 0;
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
            treeRepository.State = State.SavedOrLoaded;
            result += SaveChanges(treeRepository.ChildTreeRoots);
            return result;
        }
        /// <summary>
        /// Сохранить изменения
        /// </summary>
        /// <param name="treeRoots">Корни для сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeRootModel> treeRoots)
        {
            if (treeRoots == null || treeRoots.Count() == 0)
                return 0;
            foreach (var treeRoot in treeRoots)
            {
                if (treeRoot.State == State.Initialized && _mainEntityCollection.DataTreeRoots.Any(x => x.Guid == treeRoot.Guid) == false)
                {
                    _mainEntityCollection.DataTreeRoots.Add(treeRoot);
                }
            }
            long result = 0;
            foreach (var storage in treeRoots.Select(x => x.DataStorage).Distinct())
            {
                List<TreeRoot> dbCollection;
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.InsertRoots(dbCollection);
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.UpdateRoots(dbCollection);
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.DeleteRoots(dbCollection);
            }
            result += SaveChanges(treeRoots.SelectMany(x => x.ChildTreeNodes));
            result += SaveChanges(treeRoots.SelectMany(x => x.PersonalAttributes));
            foreach (var treeRoot in treeRoots)
            {
                SetModelState(treeRoot, State.SavedOrLoaded);
            }
            return result;
        }
        /// <summary>
        /// Сохранить изменения
        /// </summary>
        /// <param name="treeNodes">Узлы для сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeNodeModel> treeNodes)
        {
            if (treeNodes == null || treeNodes.Count() == 0)
                return 0;
            foreach (var treeNode in treeNodes)
            {
                if (treeNode.State == State.Initialized && _mainEntityCollection.DataTreeRoots.Any(x => x.Guid == treeNode.Guid) == false)
                {
                    _mainEntityCollection.DataTreeNodes.Add(treeNode);
                }
            }
            long result = 0;
            foreach (var storage in treeNodes.Select(x => x.DataStorage).Distinct())
            {
                List<TreeNode> dbCollection;
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.InsertNodes(dbCollection);
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.UpdateNodes(dbCollection);
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.DeleteNodes(dbCollection);
            }
            result += SaveChanges(treeNodes.SelectMany(x => x.ChildTreeNodes));
            result += SaveChanges(treeNodes.SelectMany(x => x.ChildTreeLeaves));
            result += SaveChanges(treeNodes.SelectMany(x => x.PersonalAttributes));
            foreach (var treeNode in treeNodes)
            {
                SetModelState(treeNode, State.SavedOrLoaded);
            }
            return result;
        }
        /// <summary>
        /// Сохранить изменения
        /// </summary>
        /// <param name="treeLeaves">Листы для сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeLeaveModel> treeLeaves)
        {
            if (treeLeaves == null || treeLeaves.Count() == 0)
                return 0;
            foreach (var treeLeave in treeLeaves)
            {
                if (treeLeave.State == State.Initialized && _mainEntityCollection.DataTreeLeaves.Any(x => x.Guid == treeLeave.Guid) == false)
                {
                    _mainEntityCollection.DataTreeLeaves.Add(treeLeave);
                }
            }
            long result = 0;
            foreach (var storage in treeLeaves.Select(x => x.DataStorage).Distinct())
            {
                List<TreeLeave> dbCollection;
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.InsertLeaves(dbCollection);
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.UpdateLeaves(dbCollection);
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.DeleteLeaves(dbCollection);
            }
            result += SaveChanges(treeLeaves.SelectMany(x => x.PersonalAttributes));
            foreach (var treeLeave in treeLeaves)
            {
                SetModelState(treeLeave, State.SavedOrLoaded);
            }
            return result;
        }
        /// <summary>
        /// Сохранить изменения
        /// </summary>
        /// <param name="elementAttributes">Атрибуты для сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<ElementAttributeModel> elementAttributes)
        {
            if (elementAttributes == null || elementAttributes.Count() == 0)
                return 0;
            //TODO: Вынести в отдельный метод AddInitialized
            foreach (var elementAttribute in elementAttributes)
            {
                if (elementAttribute.State == State.Initialized && _mainEntityCollection.ElementAttributes.Any(x => x.Guid == elementAttribute.Guid) == false)
                {
                    _mainEntityCollection.ElementAttributes.Add(elementAttribute);
                }
            }
            //TODO: Вынести в отдельный метод AddInitialized
            long result = 0;
            foreach (var storage in elementAttributes.Select(x => x.DataStorage).Distinct())
            {
                List<ElementAttribute> dbCollection;
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.Initialized).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.InsertAttributes(dbCollection);
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.Changed).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.UpdateAttributes(dbCollection);
                dbCollection = elementAttributes.Where(x => x.DataStorage == storage && x.State == State.ForSoftDelete).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.DeleteAttributes(dbCollection);
            }
            //TODO: Вынести в отдельный метод RemoveDeleted
            //TODO: ПОЧИНИТЬ!!!
            foreach (var elementAttribute in elementAttributes)
            {
                if ((elementAttribute.State == State.ForHardDelete || elementAttribute.State == State.ForSoftDelete || elementAttribute.State == State.SoftDeleted) 
                    && _mainEntityCollection.Any(x => x.Guid == elementAttribute.Guid) == false)
                {
                    _mainEntityCollection.Remove(elementAttribute);
                }
            }
            //TODO: Вынести в отдельный метод RemoveDeleted
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
