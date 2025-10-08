using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Factories;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Helpers.InfrastructureConverters;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Philadelphus.Business.Services
{
    #region [ Props ]

    public class DataTreeProcessingService
    {
        private TreeRepositoryModel _currentRepository;
        public TreeRepositoryModel CurrentRepository 
        { 
            get
            {
                return LoadRepositoryContent(_currentRepository);
            }
            set => _currentRepository = value;
        }

        private MainEntitiesCollectionModel _mainEntityCollection = new MainEntitiesCollectionModel();
        public MainEntitiesCollectionModel MainEntityCollection { get => _mainEntityCollection; }

        #endregion
















        #region [ Load ]

        public IEnumerable<TreeRepositoryModel> LoadRepositories(IEnumerable<IDataStorageModel> dataStorages)
        {
            var result = new List<TreeRepositoryModel>();
            foreach (var dataStorage in dataStorages)
            {
                var infrastructure = dataStorage.TreeRepositoryHeadersInfrastructureRepository;
                if (infrastructure.GetType().IsAssignableTo(typeof(ITreeRepositoriesInfrastructureRepository))
                    && dataStorage.IsAvailable)
                {
                    var dbRepositories = infrastructure.SelectRepositories();
                    var repositories = dbRepositories?.ToModelCollection(dataStorages);
                    if (repositories != null)
                    {
                        result.AddRange(repositories);
                    }
                }
            }
            return result;
        }
        public TreeRepositoryModel LoadRepositoryContent(TreeRepositoryModel repository)
        {
            var result = repository;
            foreach (var dataStorage in repository?.DataStorages)
            {
                var infrastructure = dataStorage.MainEntitiesInfrastructureRepository;
                if (infrastructure.GetType().IsAssignableTo(typeof(IMainEntitiesInfrastructureRepository))
                    && dataStorage.IsAvailable)
                {
                    var dbRoots = infrastructure.SelectRoots(repository.ChildsGuids.ToArray());
                    var roots = dbRoots?.ToModelCollection(repository.DataStorages);
                    if (roots != null)
                    {
                        result.Childs = roots;
                    }
                }
            }
            return result;
        }


        #endregion

        #region [ Init + Add ]

        public TreeRepositoryModel CreateNewTreeRepository(IDataStorageModel dataStorage)
        {
            var result = new TreeRepositoryModel(Guid.NewGuid(), dataStorage);
            //((ITreeRepositoriesInfrastructureRepository)result.OwnDataStorage).InsertRepository(result.ToDbEntity());
            return result;
        }
        public IEnumerable<TreeRepositoryModel> AddExistTreeRepository(DirectoryInfo path)
        {
            var result = new List<TreeRepositoryModel>();
            return result;
        }
        public TreeRootModel CreateTreeRoot(TreeRepositoryModel parentElement, IDataStorageModel dataStorage)
        {
            var result = new TreeRootModel(Guid.NewGuid(), parentElement, dataStorage);
            ((List<TreeRepositoryMemberBaseModel>)parentElement.ElementsCollection).Add(result);
            ((ObservableCollection<IChildrenModel>)parentElement.Childs).Add(result);
            return result;
        }
        public TreeNodeModel CreateTreeNode(IParentModel parentElement)
        {
            var result = new TreeNodeModel(Guid.NewGuid(), parentElement);
            ((List<TreeRepositoryMemberBaseModel>)result.ParentRepository.ElementsCollection).Add(result);
            ((ObservableCollection<IChildrenModel>)parentElement.Childs).Add(result);
            return result;
        }
        public TreeLeaveModel CreateTreeLeave(IParentModel parentElement)
        {
            try
            {
                if (parentElement.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)) == false || parentElement.GetType() != typeof(TreeNodeModel))
                {
                    NotificationService.SendNotification("Лист можно добавить только в узел.", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
                    return null;
                }
                else
                {
                    var result = new TreeLeaveModel(Guid.NewGuid(), parentElement);
                    ((List<TreeRepositoryMemberBaseModel>)result.ParentRepository.ElementsCollection).Add(result);
                    ((ObservableCollection<IChildrenModel>)parentElement.Childs).Add(result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                NotificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
                throw;
            }
        }
        public ElementAttributeModel CreateElementAttribute(IContentOwnerModel owner)
        {
            var result = new ElementAttributeModel(Guid.NewGuid(), owner);
            //((List<ITreeRepositoryMember>)result.ParentRepository.ElementsCollection).Add(result);
            ((List<ElementAttributeModel>)owner.PersonalAttributes).Add(result);
            return result;
        }

        #endregion

        #region [ Delete + Remove ]

        public bool RemoveMember(IChildrenModel element)
        {
            try
            {
                if (element == null)
                {
                    return false;
                }
                ((ObservableCollection<IChildrenModel>)element.Parent.Childs).Remove(element);
                if (element.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)) && element.GetType().IsAssignableTo(typeof(TreeRepositoryMemberBaseModel)))
                {
                    ((List<TreeRepositoryMemberBaseModel>)((ITreeRepositoryMemberModel)element).ParentRepository.ElementsCollection).Remove((TreeRepositoryMemberBaseModel)element);
                }
                //
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region [ Save ]

        public long SaveChanges()
        {
            long result = 0;
            result = SaveChanges(_currentRepository);
            return result;
        }
        public long SaveChanges(TreeRepositoryModel treeRepository)
        {
            long result = 0;
            switch (treeRepository.State)
            {
                case State.Initialized:
                    treeRepository.OwnDataStorage.TreeRepositoryHeadersInfrastructureRepository.InsertRepository(treeRepository.ToDbEntity());
                    break;
                case State.Changed:
                    treeRepository.OwnDataStorage.TreeRepositoryHeadersInfrastructureRepository.UpdateRepository(treeRepository.ToDbEntity());
                    break;
                case State.Deleted:
                    result = treeRepository.OwnDataStorage.TreeRepositoryHeadersInfrastructureRepository.DeleteRepository(treeRepository.ToDbEntity());
                    break;
                default:
                    break;
            }
            treeRepository.State = State.Saved;
            for (int i = 0; i < treeRepository.Childs.Count(); i++)
            {
                SaveChanges(((TreeRootModel)treeRepository.Childs.ToList()[i]));
            }
            return result;
        }
        public long SaveChanges(TreeRootModel treeRoot)
        {
            long result = 0;
            switch (treeRoot.State)
            {
                case State.Initialized:
                    treeRoot.OwnDataStorage.MainEntitiesInfrastructureRepository.InsertRoots(new List<TreeRoot>() { treeRoot.ToDbEntity() });
                    break;
                case State.Changed:
                    treeRoot.OwnDataStorage.MainEntitiesInfrastructureRepository.UpdateRoots(new List<TreeRoot>() { treeRoot.ToDbEntity() });
                    break;
                case State.Deleted:
                    result = treeRoot.OwnDataStorage.MainEntitiesInfrastructureRepository.DeleteRoots(new List<TreeRoot>() { treeRoot.ToDbEntity() });
                    break;
                default:
                    break;
            }
            treeRoot.State = State.Saved;
            for (int i = 0; i < treeRoot.Childs.Count(); i++)
            {
                SaveChanges(((TreeNodeModel)treeRoot.Childs.ToList()[i]));
            }
            return result;
        }
        public long SaveChanges(TreeNodeModel treeNode)
        {
            long result = 0;

            //for (int i = 0; i < treeNode.Childs.Count(); i++)
            //{
            //    SaveChanges(((TreeLeaveModel)treeNode.Childs.ToList()[i]));
            //}
            treeNode.State = State.Saved;
            return result;
        }
        public long SaveChanges(TreeLeaveModel treeLeave)
        {
            long result = 0;

            treeLeave.State = State.Saved;
            return result;
        }

        #endregion

        #region [ Temp ]

        /// <summary>
        /// Создание примера репозитория.
        /// </summary>
        /// <returns></returns>
        public TreeRepositoryModel CreateSampleRepository(IDataStorageModel dataStorage)
        {
            var repo = new TreeRepositoryModel(Guid.NewGuid(), dataStorage);
            _mainEntityCollection.DataTreeRepositories.Add(repo);
            for (int i = 0; i < 5; i++)
            {
                var root = new TreeRootModel(Guid.NewGuid(), repo, dataStorage);
                GetAttributesSample(root);
                ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(root);
                for (int j = 0; j < 5; j++)
                {
                    var node = new TreeNodeModel(Guid.NewGuid(), root);
                    GetAttributesSample(node);
                    ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(node);
                    for (int k = 0; k < 5; k++)
                    {
                        var node2 = new TreeNodeModel(Guid.NewGuid(), root);
                        GetAttributesSample(node2);
                        ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(node2);
                        ((ObservableCollection<IChildrenModel>)node.Childs).Add(node2);
                        var leave = new TreeLeaveModel(Guid.NewGuid(), node);
                        GetAttributesSample(leave);
                        ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(leave);
                        ((ObservableCollection<IChildrenModel>)node.Childs).Add(leave);
                    }
                    ((ObservableCollection<IChildrenModel>)root.Childs).Add(node);
                }
                ((ObservableCollection<IChildrenModel>)repo.Childs).Add(root);
            }
            return repo;
        }
        private List<ElementAttributeModel> GetAttributesSample(IContentOwnerModel owner)
        {
            var result = new List<ElementAttributeModel>();

            for (int i = 0; i < 20; i++)
            {
                var entry = new ElementAttributeModel(Guid.NewGuid(), owner);
                ((List<ElementAttributeModel>)owner.PersonalAttributes).Add(entry);
                result.Add(entry);
            }

            return result;
        }
        #endregion
    }
}
