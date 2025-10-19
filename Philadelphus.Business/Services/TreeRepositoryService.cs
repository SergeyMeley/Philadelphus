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

    public class TreeRepositoryService
    {
        private ITreeRepositoryHeaderModel _repository;
        public ITreeRepositoryHeaderModel Repository 
        { 
            get
            {
                return _repository;
            }
        }

        private static MainEntitiesCollectionModel _mainEntityCollection = new MainEntitiesCollectionModel();
        public static MainEntitiesCollectionModel MainEntityCollection { get => _mainEntityCollection; }

        #endregion

        #region [ Construct ]

        public TreeRepositoryService(ITreeRepositoryHeaderModel repository)
        {
            _repository = repository;
        }

        #endregion

        #region [ Get + Load ]

        public static IMainEntity GetEntityFromCollection(Guid guid)
        {
            return GetModelFromCollection(guid).ToDbEntity();
        }
        public static IMainEntityModel GetModelFromCollection(Guid guid)
        {
            return _mainEntityCollection.FirstOrDefault(x => x.Guid == guid);
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
                    var dbRoots = infrastructure.SelectRoots(repository.ChildsGuids?.ToArray());
                    var roots = dbRoots?.ToModelCollection(repository.DataStorages);
                    if (roots != null)
                    {
                        for (int i = 0; i < roots.Count; i++)
                        {
                            roots[i].State = State.SavedOrLoaded;
                        }
                        result.Childs = roots.Cast<IChildrenModel>().ToList();
                    }
                }
            }
            return result;
        }

        #endregion

        #region [ Save ]

        public long SaveChanges()
        {
            long result = 0;
            result = SaveChanges(_repository as TreeRepositoryModel);
            return result;
        }
        public long SaveChanges(TreeRepositoryModel treeRepository)
        {
            if (treeRepository == null)
                return 0;
            long result = 0;
            switch (treeRepository.State)
            {
                case State.Initialized:
                    result += treeRepository.OwnDataStorage.TreeRepositoryHeadersInfrastructureRepository.InsertRepository(treeRepository.ToDbEntity());
                    break;
                case State.Changed:
                    result += treeRepository.OwnDataStorage.TreeRepositoryHeadersInfrastructureRepository.UpdateRepository(treeRepository.ToDbEntity());
                    break;
                case State.Deleted:
                    result += treeRepository.OwnDataStorage.TreeRepositoryHeadersInfrastructureRepository.DeleteRepository(treeRepository.ToDbEntity());
                    break;
                default:
                    break;
            }
            treeRepository.State = State.SavedOrLoaded;
            result += SaveChanges(treeRepository.ChildTreeRoots);
            return result;
        }
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
                dbCollection = treeRoots.Where(x => x.DataStorage == storage && x.State == State.Deleted).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.DeleteRoots(dbCollection);
            }
            result += SaveChanges(treeRoots.SelectMany(x => x.ChildTreeNodes));
            foreach (var treeRoot in treeRoots)
            {
                treeRoot.State = State.SavedOrLoaded;
            }
            return result;
        }
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
                dbCollection = treeNodes.Where(x => x.DataStorage == storage && x.State == State.Deleted).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.DeleteNodes(dbCollection);
            }
            result += SaveChanges(treeNodes.SelectMany(x => x.ChildTreeNodes));
            result += SaveChanges(treeNodes.SelectMany(x => x.ChildTreeLeaves));
            foreach (var treeNode in treeNodes)
            {
                treeNode.State = State.SavedOrLoaded;
            }
            return treeNodes.Where(x => x.State != State.SavedOrLoaded).Count();
        }
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
                dbCollection = treeLeaves.Where(x => x.DataStorage == storage && x.State == State.Deleted).ToDbEntityCollection();
                result += storage.MainEntitiesInfrastructureRepository.DeleteLeaves(dbCollection);
            }
            foreach (var treeLeave in treeLeaves)
            {
                treeLeave.State = State.SavedOrLoaded;
            }
            return result;
        }

        #endregion

        #region [ Create + Add ]

        public TreeRootModel CreateTreeRoot(TreeRepositoryModel parentElement, IDataStorageModel dataStorage)
        {
            var result = new TreeRootModel(Guid.NewGuid(), parentElement, dataStorage);
            parentElement.ElementsCollection.Add(result);
            parentElement.Childs.Add(result);
            return result;
        }
        public TreeNodeModel CreateTreeNode(IParentModel parentElement)
        {
            var result = new TreeNodeModel(Guid.NewGuid(), parentElement);
            result.ParentRepository.ElementsCollection.Add(result);
            parentElement.Childs.Add(result);
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
                    result.ParentRepository.ElementsCollection.Add(result);
                    parentElement.Childs.Add(result);
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
            owner.PersonalAttributes.Add(result);
            return result;
        }

        #endregion

        #region [ Modify ]

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
                element.Parent.Childs.Remove(element);
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

        #region [ Temp ]

        internal List<ElementAttributeModel> GetAttributesSample(IContentOwnerModel owner)
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
