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
    public class DataTreeProcessingService
    {
        private TreeRepositoryModel _currentRepository;
        public TreeRepositoryModel CurrentRepository 
        { 
            get
            {
                return GetRepositoryContent(_currentRepository);
            }
            set => _currentRepository = value;
        }

        private MainEntitiesCollectionModel _mainEntityCollection = new MainEntitiesCollectionModel();

        public MainEntitiesCollectionModel MainEntityCollection { get => _mainEntityCollection; }

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
            SaveChanges((TreeRootModel)treeRepository.Childs);
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
            SaveChanges((TreeNodeModel)treeRoot.Childs);
            return result;
        }
        public long SaveChanges(TreeNodeModel treeNode)
        {
            long result = 0;

            SaveChanges((TreeLeaveModel)treeNode.Childs);
            return result;
        }
        public long SaveChanges(TreeLeaveModel treeLeave)
        {
            long result = 0;


            return result;
        }

        public long SaveChanges()
        {
            long result = 0;
            result = SaveChanges(_currentRepository);
            return result;
        }

        public IEnumerable<TreeRepositoryModel> GetRepositories(IEnumerable<IDataStorageModel> dataStorages)
        {
            var result = new List<TreeRepositoryModel>();
            foreach (var dataStorage in dataStorages)
            {
                var infrastructure = dataStorage.TreeRepositoryHeadersInfrastructureRepository;
                if (dataStorage.TreeRepositoryHeadersInfrastructureRepository.GetType().IsAssignableTo(typeof(ITreeRepositoriesInfrastructureRepository))
                    && dataStorage.IsAvailable)
                {
                    result.AddRange(infrastructure.SelectRepositories().ToModelCollection(dataStorages));
                }
            }
            return result;
        }

        //public List<TreeRepository> AddRepository(TreeRepository repository)
        //{
        //    if (repository != null)
        //    {
        //        var infrastructure = new WindowsFileSystemRepository.Repositories.WindowsMainEntityRepository();
        //        // Добавление пути к папке и файлу репозитория
        //        //repository.DirectoryFullPath = Path.Join(new string[] { repository.DirectoryPath, Path.DirectorySeparatorChar.ToString(), repository.Name });
        //        //repository.ConfigPath = Path.Join(new string[] { repository.DirectoryFullPath, Path.DirectorySeparatorChar.ToString(), ".repository" });
        //        //// Получение текущего списка репозиториев и добавление туда нового
        //        //DataTreeRepositories = GetRepositoryHeadersCollection();
        //        //DataTreeRepositories.Add(repository);
        //        // Создания нового репозитория
        //        var list = new List<TreeRepository>();
        //        list.Add(repository);
        //        var converter = new RepositoryInfrastructureConverter();
        //        infrastructure.InsertRepository(converter.ToDbEntityCollection(list));
        //        // Дополнение списка репозиториев в настроечном файле
        //        //GeneralSettings.RepositoryPathList = DataTreeRepositories.Select(x => x.ConfigPath).Distinct().ToList();
        //        //infrastructure.InsertRepositoryPathes(GeneralSettings.RepositoryListPath, GeneralSettings.RepositoryPathList);
        //        // Получение актуального списка репозиториев
        //    }
        //    return GetRepositoryHeadersCollection();
        //}
        public TreeRootModel InitTreeRoot(TreeRepositoryModel parentElement, IDataStorageModel dataStorage)
        {
            var result = new TreeRootModel(Guid.NewGuid(), parentElement, dataStorage);
            ((List<TreeRepositoryMemberBaseModel>)parentElement.ElementsCollection).Add(result);
            ((ObservableCollection<IChildrenModel>)parentElement.Childs).Add(result);
            return result;
        }
        public TreeNodeModel InitTreeNode(IParentModel parentElement)
        {
            var result = new TreeNodeModel(Guid.NewGuid(), parentElement);
            ((List<TreeRepositoryMemberBaseModel>)result.ParentRepository.ElementsCollection).Add(result);
            ((ObservableCollection<IChildrenModel>)parentElement.Childs).Add(result);
            return result;
        }
        public TreeLeaveModel InitTreeLeave(IParentModel parentElement)
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

        public ElementAttributeModel InitElementAttribute(IContentOwnerModel owner)
        {
            var result = new ElementAttributeModel(Guid.NewGuid(), owner);
            //((List<ITreeRepositoryMember>)result.ParentRepository.ElementsCollection).Add(result);
            ((List<ElementAttributeModel>)owner.PersonalAttributes).Add(result);
            return result;
        }

        public bool RemoveElement(IChildrenModel element)
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
        //public List<TreeRepository> ModifyRepository(TreeRepository repository)
        //{
        //    return AddRepository(repository);
        //    //using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
        //    //{
        //    //    xmlSerializer.Serialize(fs, RepositoryPathList);
        //    //}
        //    //using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
        //    //{
        //    //    //var dbRepositories = new List<DbTreeRepository>();
        //    //    try
        //    //    {
        //    //        RepositoryPathList = xmlSerializer.Deserialize(fs) as List<string>;
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //    }
        //    //    if (RepositoryPathList != null)
        //    //    {
        //    //        var repositoryXmlSerializer = new XmlSerializer(typeof(List<string>));
        //    //        foreach (var item in RepositoryPathList)
        //    //        {
        //    //        }
        //    //        //var repositoryInfrastructureConverter = new RepositoryInfrastructureConverter();
        //    //        //DataTreeRepositories = (List<TreeRepository>)repositoryInfrastructureConverter.ToModelCollection(dbRepositories);
        //    //    }
        //    //    return GetRepositoryHeadersCollection();
        //    //}
        //}
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

        public TreeRepositoryModel CreateNewTreeRepository(string name, IDataStorageModel dataStorage)
        {
            var result = new TreeRepositoryModel(Guid.NewGuid(), dataStorage);
            if (string.IsNullOrEmpty(name))
            {
                name = "Новый репозиторий";
            }
            result.Name = NamingHelper.GetNewName(null, name);

            ((ITreeRepositoriesInfrastructureRepository)result.OwnDataStorage).InsertRepository(result.ToDbEntity());
            return result;
        }

        

        public IEnumerable<TreeRepositoryModel> AddExistTreeRepository(DirectoryInfo path)
        {
            var result = new List<TreeRepositoryModel>();
            return result;
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
        /// <summary>
        /// Сохранение измененного репозитория.
        /// </summary>
        /// <param name="entity"></param>
        //public void SaveRepository(TreeRepository repository)
        //{
        //    DataTreeRepositories = GetRepositoryHeadersCollection();
        //    bool availableInList = false;
        //    for (int i = 0; i < DataTreeRepositories.Count; i++)
        //    {
        //        if (DataTreeRepositories[i].Guid == repository.Guid)
        //        {
        //            DataTreeRepositories[i] = repository;
        //            availableInList = true;
        //        }
        //    }
        //    if (!availableInList)
        //    {
        //        DataTreeRepositories.Add(repository);
        //    }
        //    foreach (var item in DataTreeRepositories)
        //    {
        //        item.DirectoryFullPath = Path.Join(new string[] { item.DirectoryPath, Path.DirectorySeparatorChar.ToString(), item.Name });
        //        item.ConfigPath = Path.Join(new string[] { item.DirectoryFullPath, Path.DirectorySeparatorChar.ToString(), ".repository" });
        //    }
        //    UpdateEntities(DataTreeRepositories);
            
        //}
        //private void UpdateEntities(IEnumerable<IMainEntityModel> entities, InfrastructureTypes infrastructure)
        //{
        //    foreach (var entityType in entities.Select(x => x.EntityType).Distinct())
        //    {
        //        var infrastructureRepository = InfrastructureFactory.GetMainEntitiesInfrastructure(infrastructure);
        //        InfrastructureConverterBase converter;
        //        switch (entityType)
        //        {
        //            case EntityTypesModel.None:
        //                break;
        //            case EntityTypesModel.Repository:
        //                converter = new RepositoryInfrastructureConverter();
        //                ((ITreeRepositoriesInfrastructureRepository)infrastructureRepository).UpdateRepository((List<TreeRepository>)converter.ToDbEntityCollection(entities));
        //                break;
        //            case EntityTypesModel.Root:
        //                converter = new RootInfrastructureConverter();
        //                infrastructureRepository.UpdateRoots((List<TreeRoot>)converter.ToDbEntityCollection(entities));
        //                break;
        //            case EntityTypesModel.Node:
        //                converter = new NodeInfrastructureConverter();
        //                infrastructureRepository.UpdateNodes((List<TreeNode>)converter.ToDbEntityCollection(entities));
        //                break;
        //            case EntityTypesModel.Leave:
        //                converter = new LeaveInfrastructureConverter();
        //                infrastructureRepository.UpdateLeaves((List<TreeLeave>)converter.ToDbEntityCollection(entities));
        //                break;
        //            case EntityTypesModel.Attribute:
        //                converter = new AttributeInfrastructureConverter();
        //                infrastructureRepository.UpdateAttributes((List<ElementAttribute>)converter.ToDbEntityCollection(entities));
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}









        private TreeRepositoryModel GetRepositoryContent(TreeRepositoryModel currentRepository)
        {
            //foreach (var item in CurrentRepository.RootsDefaultDataStorage)
            //{
            //    IMainEntitiesInfrastructureRepository infrastructureRepository;
            //    switch (item.InfrastructureRepositoryTypes)
            //    {
            //        case InfrastructureTypes.WindowsDirectory:
            //            infrastructureRepository = new WindowsFileSystemRepository.Repositories.WindowsMainEntityRepository();
            //            break;
            //        case InfrastructureTypes.PostgreSql:
            //            infrastructureRepository = new PostgreInfrastructure.Repositories.PostgreMainEntityInfrastructure();
            //            break;
            //        case InfrastructureTypes.MongoDbAdo:
            //            infrastructureRepository = new MongoRepository.Repositories.MongoMainEntitуInfrastructure();
            //            break;
            //        default:
            //            infrastructureRepository = null;
            //            break;
            //    }
            //    var nodeInfrastructureConverter = new NodeInfrastructureConverter();
            //    _dbMainEntitiesCollection.DbTreeNodes = infrastructureRepository.SelectNodes();
            //    var leaveInfrastructureConverter = new LeaveInfrastructureConverter();
            //    _dbMainEntitiesCollection.DbTreeLeaves = infrastructureRepository.SelectLeaves();
            //}
            _currentRepository = _mainEntityCollection.DataTreeRepositories.Where(x => x.Guid == currentRepository.Guid).Last();
            //using (var fs = new FileStream(CurrentRepository.ConfigPath, FileMode.OpenOrCreate))
            //{
            //    //var dbRepository = new DbTreeRepository();
            //    //try
            //    //{
            //    //    dbRepository.ChildTreeRootIds = xmlSerializer.Deserialize(fs) as List<TreeRoot>;
            //    //}
            //    //catch (Exception ex)
            //    //{
            //    //}
            //    //if (dbRepository != null)
            //    //{

            //    //}
            //}
            for (int i = 0; i < currentRepository.Childs.Count(); i++)
            {
                //currentRepository.ChildTreeRoots.ToList()[i] = GetRootContent(currentRepository.ChildTreeRoots.ToList()[i]);
            }
            return _currentRepository;
        }
        //private TreeRoot GetRootContent(TreeRoot treeRoot) 
        //{
        //    IMainEntitiesInfrastructureRepository infrastructureRepository = InfrastructureFactory.GetMainEntitiesInfrastructure(treeRoot.InftastructureRepositoryType);
        //    var nodeCollection = infrastructureRepository.SelectNodes(InfrastructureConverterBase.BusinessToDbRepository(CurrentRepository)).Where(x => x.Guid == treeRoot.Guid);
        //    for (int i = 0; i < nodeCollection.Count(); i++)
        //    {
        //        //treeRoot.ChildTreeNodes.ToList()[i] = InfrastructureConverter.DbToBusinessNode(nodeCollection.ToList()[i]);
        //    }
        //    return treeRoot;
        //}
        //private TreeNode GetNodeContent(TreeNode treeNode)
        //{
        //    IMainEntitiesInfrastructureRepository infrastructureRepository = new MongoRepository.Repositories.MongoMainEntitуInfrastructure();
        //    var nodeCollection = infrastructureRepository.SelectNodes(InfrastructureConverterBase.BusinessToDbRepository(CurrentRepository)).Where(x => x.Guid == treeNode.Guid);
        //    for (int i = 0; i < nodeCollection.Count(); i++)
        //    {
        //        treeNode.ChildTreeNodes.ToList()[i] = InfrastructureConverterBase.DbToBusinessNode(nodeCollection.ToList()[i]);
        //    }
        //    return treeNode;
        //}
    }
}
