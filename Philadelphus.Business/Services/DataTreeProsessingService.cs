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
using System.Xml.Linq;
using System.Xml.Serialization;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.Business.Helpers.InfrastructureConverters;

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
        public List<TreeRepository> DataTreeRepositoryList { get; private set; } = new List<TreeRepository>();
        private DbMainEntitiesCollection _dbMainEntitiesCollection = new DbMainEntitiesCollection();
        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DbTreeRepository>));
        public List<TreeRepository> GetRepositoryList()
        {
            using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
            {
                var dbRepositories = new List<DbTreeRepository>();
                try
                {
                    dbRepositories = xmlSerializer.Deserialize(fs) as List<DbTreeRepository>;
                }
                catch (Exception ex)
                {
                }
                if (dbRepositories != null)
                {
                    var repositoryInfrastructureConverter = new RepositoryInfrastructureConverter();
                    DataTreeRepositoryList = (List<TreeRepository>)repositoryInfrastructureConverter.DbToBusinessEntityCollection(dbRepositories);
                }
                return DataTreeRepositoryList;
            }
        }
        public TreeRepository CreateRepository()
        {
            CurrentRepository = (TreeRepository)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Repository);
            return CurrentRepository;
        }
        public void ModifyRepository(TreeRepository repository)
        {
            GetRepositoryList();

            DataTreeRepositoryList.Add(repository);
                
            using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
            {
                foreach (var item in DataTreeRepositoryList)
                {
                    item.DirectoryFullPath = Path.Join(new string[] { item.DirectoryPath, Path.DirectorySeparatorChar.ToString(), item.Name });
                    item.ConfigPath = Path.Join(new string[] { item.DirectoryFullPath, Path.DirectorySeparatorChar.ToString(), ".repository" });
                }
                SaveTreeEntities(DataTreeRepositoryList);
                var repositoryInfrastructureConverter = new RepositoryInfrastructureConverter();
                var result = repositoryInfrastructureConverter.BusinessToDbEntityCollection(DataTreeRepositoryList);
                xmlSerializer.Serialize(fs, result);
            }
        }
        public void SaveTreeEntities(IEnumerable<IMainEntity> entities)
        {
            foreach (var entityType in entities.Select(x => x.EntityType).Distinct())
            {
                var infrastructureRepository = InfrastructureFactory.CreateMainEntitiesRepositoriesFactory(entities.Last().InfrastructureRepositoryType);
                //RepositoryInfrastructureConverter
                //var dbEntityList = new List<IDbEntity>();
                //foreach (var entity in entities.Where(x => x.EntityType == entityType))
                //{
                //    //dbEntityList.Add(InfrastructureConverterBase.);
                //}
                //switch (entityType)
                //{
                //    case EntityTypes.None:
                //        break;
                //    case EntityTypes.Repository:
                //        infrastructureRepository.UpdateRepositories((List<DbTreeRepository>)dbEntityList);
                //        break;
                //    case EntityTypes.Root:
                //        infrastructureRepository.UpdateRoots((List<DbTreeRoot>)dbEntityList);
                //        break;
                //    case EntityTypes.Node:
                //        infrastructureRepository.UpdateNodes((List<DbTreeNode>)dbEntityList);
                //        break;
                //    case EntityTypes.Leave:
                //        infrastructureRepository.UpdateRepositories((List<DbTreeRepository>)dbEntityList);
                //        break;
                //    case EntityTypes.Attribute:
                //        infrastructureRepository.UpdateAttributes((List<DbAttribute>)dbEntityList);
                //        break;
                //    default:
                //        break;
                //}
            }
            // Проверяем указанный путь. Если его нет, пробуем создать.
            //if (!Directory.Exists(item.DirectoryFullPath))
            //{
            //    try
            //    {
            //        Directory.CreateDirectory(item.DirectoryFullPath);
            //    }
            //    catch (Exception)
            //    {
            //        throw new Exception("Ошибка создания директории, проверьте указанный путь");
            //    }
            //    if (!File.Exists(item.ConfigPath))
            //    {
            //        try
            //        {
            //            File.Create(item.ConfigPath);
            //        }
            //        catch (Exception)
            //        {
            //            throw new Exception("Ошибка создания элемента, проверьте указанный путь");
            //        }
            //    }
            //}
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
                _dbMainEntitiesCollection.DbTreeNodes = infrastructureRepository.SelectNodes((DbTreeRepository)nodeInfrastructureConverter.BusinessToDbEntity(currentRepository));
                var leaveInfrastructureConverter = new LeaveInfrastructureConverter();
                _dbMainEntitiesCollection.DbTreeLeaves = infrastructureRepository.SelectLeaves((DbTreeRepository)nodeInfrastructureConverter.BusinessToDbEntity(currentRepository));
            }
            CurrentRepository = DataTreeRepositoryList.Where(x => x.Guid == currentRepository.Guid).Last();
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
        //    IMainEntitiesRepository infrastructureRepository = InfrastructureFactory.CreateMainEntitiesRepositoriesFactory(treeRoot.InftastructureRepositoryType);
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
