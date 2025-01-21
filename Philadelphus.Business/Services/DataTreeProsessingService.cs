using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Factories;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.Business.Helpers.InfrastructureConverters;
using System.Xml.Serialization;

namespace Philadelphus.Business.Services
{
    public class DataTreeRepositoryService
    {
        public TreeRepository CurrentRepository 
        { 
            get
            {
                GetRepositoryContent(CurrentRepository);
                return CurrentRepository;
            }
            set => CurrentRepository = value;
        }
        public List<TreeRepository> DataTreeRepositories { get; private set; } = new List<TreeRepository>();
        private DbMainEntitiesCollection _dbMainEntitiesCollection = new DbMainEntitiesCollection();
        public List<TreeRepository> GetRepositories()
        {
            var infrastructure = new WindowsFileSystemRepository.Repositories.MainEntityRepository();
            // Получение списка путей к репозиториям
            GeneralSettings.RepositoryPathList = (List<string>)infrastructure.SelectRepositoryPathes(GeneralSettings.RepositoryListPath);
            // Получение репозиториев по всем путям
            if (GeneralSettings.RepositoryPathList != null)
            {
                var dbRepositories = (List<DbTreeRepository>)infrastructure.SelectRepositories(GeneralSettings.RepositoryPathList);
                var converter = new RepositoryInfrastructureConverter();
                DataTreeRepositories = converter.DbToBusinessEntityCollection(dbRepositories);
            }
            return DataTreeRepositories;
        }
        public List<TreeRepository> AddRepository(TreeRepository repository)
        {
            if (repository != null)
            {
                var infrastructure = new WindowsFileSystemRepository.Repositories.MainEntityRepository();
                // Добавление пути к папке и файлу репозитория
                repository.DirectoryFullPath = Path.Join(new string[] { repository.DirectoryPath, Path.DirectorySeparatorChar.ToString(), repository.Name });
                repository.ConfigPath = Path.Join(new string[] { repository.DirectoryFullPath, Path.DirectorySeparatorChar.ToString(), ".repository" });
                // Получение текущего списка репозиториев и добавление туда нового
                DataTreeRepositories = GetRepositories();
                DataTreeRepositories.Add(repository);
                // Создания нового репозитория
                var list = new List<TreeRepository>();
                list.Add(repository);
                var converter = new RepositoryInfrastructureConverter();
                infrastructure.InsertRepositories(converter.BusinessToDbEntityCollection(list));
                // Дополнение списка репозиториев в настроечном файле
                GeneralSettings.RepositoryPathList = DataTreeRepositories.Select(x => x.ConfigPath).Distinct().ToList();
                infrastructure.InsertRepositoryPathes(GeneralSettings.RepositoryListPath, GeneralSettings.RepositoryPathList);
                // Получение актуального списка репозиториев
            }
            return GetRepositories();
        }
        public List<TreeRepository> ModifyRepository(TreeRepository repository)
        {
            return AddRepository(repository);
            //using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
            //{
            //    xmlSerializer.Serialize(fs, RepositoryPathList);
            //}
            //using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
            //{
            //    //var dbRepositories = new List<DbTreeRepository>();
            //    try
            //    {
            //        RepositoryPathList = xmlSerializer.Deserialize(fs) as List<string>;
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //    if (RepositoryPathList != null)
            //    {
            //        var repositoryXmlSerializer = new XmlSerializer(typeof(List<string>));
            //        foreach (var item in RepositoryPathList)
            //        {
            //        }
            //        //var repositoryInfrastructureConverter = new RepositoryInfrastructureConverter();
            //        //DataTreeRepositories = (List<TreeRepository>)repositoryInfrastructureConverter.DbToBusinessEntityCollection(dbRepositories);
            //    }
            //    return GetRepositories();
            //}
        }
        /// <summary>
        /// Создание нового пустого репозитория.
        /// </summary>
        /// <returns></returns>
        public TreeRepository CreateRepository()
        {
            CurrentRepository = (TreeRepository)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Repository);
            return CurrentRepository;
        }
        /// <summary>
        /// Сохранение измененного репозитория.
        /// </summary>
        /// <param name="entity"></param>
        //public void SaveRepository(TreeRepository repository)
        //{
        //    DataTreeRepositories = GetRepositories();
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
        private void UpdateEntities(IEnumerable<IMainEntity> entities)
        {
            foreach (var entityType in entities.Select(x => x.EntityType).Distinct())
            {
                var infrastructureRepository = InfrastructureFactory.GetMainEntitiesInfrastructure(entities.Last().InfrastructureRepositoryType);
                InfrastructureConverterBase converter;
                switch (entityType)
                {
                    case EntityTypes.None:
                        break;
                    case EntityTypes.Repository:
                        converter = new RepositoryInfrastructureConverter();
                        infrastructureRepository.UpdateRepositories((List<DbTreeRepository>)converter.BusinessToDbEntityCollection(entities));
                        break;
                    case EntityTypes.Root:
                        converter = new RootInfrastructureConverter();
                        infrastructureRepository.UpdateRoots((List<DbTreeRoot>)converter.BusinessToDbEntityCollection(entities));
                        break;
                    case EntityTypes.Node:
                        converter = new NodeInfrastructureConverter();
                        infrastructureRepository.UpdateNodes((List<DbTreeNode>)converter.BusinessToDbEntityCollection(entities));
                        break;
                    case EntityTypes.Leave:
                        converter = new LeaveInfrastructureConverter();
                        infrastructureRepository.UpdateRepositories((List<DbTreeRepository>)converter.BusinessToDbEntityCollection(entities));
                        break;
                    case EntityTypes.Attribute:
                        converter = new AttributeInfrastructureConverter();
                        infrastructureRepository.UpdateAttributes((List<DbEntityAttribute>)converter.BusinessToDbEntityCollection(entities));
                        break;
                    default:
                        break;
                }
            }
        }









        private TreeRepository GetRepositoryContent(TreeRepository currentRepository)
        {
            foreach (var item in CurrentRepository.InfrastructureRepositories)
            {
                IMainEntitiesRepository infrastructureRepository;
                switch (item.InftastructureRepositoryTypes)
                {
                    case InfrastructureRepositoryTypes.WindowsDirectory:
                        infrastructureRepository = new WindowsFileSystemRepository.Repositories.MainEntityRepository();
                        break;
                    case InfrastructureRepositoryTypes.PostgreSql:
                        infrastructureRepository = new PostgreRepository.Repositories.MainEntityRepository();
                        break;
                    case InfrastructureRepositoryTypes.MongoDb:
                        infrastructureRepository = new MongoRepository.Repositories.MainEntitуRepository();
                        break;
                    default:
                        infrastructureRepository = null;
                        break;
                }
                var nodeInfrastructureConverter = new NodeInfrastructureConverter();
                _dbMainEntitiesCollection.DbTreeNodes = infrastructureRepository.SelectNodes();
                var leaveInfrastructureConverter = new LeaveInfrastructureConverter();
                _dbMainEntitiesCollection.DbTreeLeaves = infrastructureRepository.SelectLeaves();
            }
            CurrentRepository = DataTreeRepositories.Where(x => x.Guid == currentRepository.Guid).Last();
            using (var fs = new FileStream(CurrentRepository.ConfigPath, FileMode.OpenOrCreate))
            {
                //var dbRepository = new DbTreeRepository();
                //try
                //{
                //    dbRepository.ChildTreeRootIds = xmlSerializer.Deserialize(fs) as List<TreeRoot>;
                //}
                //catch (Exception ex)
                //{
                //}
                //if (dbRepository != null)
                //{

                //}
            }
            for (int i = 0; i < currentRepository.ChildTreeRoots.Count(); i++)
            {
                //currentRepository.ChildTreeRoots.ToList()[i] = GetRootContent(currentRepository.ChildTreeRoots.ToList()[i]);
            }
            return CurrentRepository;
        }
        //private TreeRoot GetRootContent(TreeRoot treeRoot) 
        //{
        //    IMainEntitiesRepository infrastructureRepository = InfrastructureFactory.GetMainEntitiesInfrastructure(treeRoot.InftastructureRepositoryType);
        //    var nodeCollection = infrastructureRepository.SelectNodes(InfrastructureConverterBase.BusinessToDbRepository(CurrentRepository)).Where(x => x.Guid == treeRoot.Guid);
        //    for (int i = 0; i < nodeCollection.Count(); i++)
        //    {
        //        //treeRoot.ChildTreeNodes.ToList()[i] = InfrastructureConverter.DbToBusinessNode(nodeCollection.ToList()[i]);
        //    }
        //    return treeRoot;
        //}
        //private TreeNode GetNodeContent(TreeNode treeNode)
        //{
        //    IMainEntitiesRepository infrastructureRepository = new MongoRepository.Repositories.MainEntitуRepository();
        //    var nodeCollection = infrastructureRepository.SelectNodes(InfrastructureConverterBase.BusinessToDbRepository(CurrentRepository)).Where(x => x.Guid == treeNode.Guid);
        //    for (int i = 0; i < nodeCollection.Count(); i++)
        //    {
        //        treeNode.ChildTreeNodes.ToList()[i] = InfrastructureConverterBase.DbToBusinessNode(nodeCollection.ToList()[i]);
        //    }
        //    return treeNode;
        //}
    }
}
