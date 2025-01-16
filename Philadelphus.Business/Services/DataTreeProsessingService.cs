using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Factories;
using Philadelphus.Business.Helpers;
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
        public List<TreeRepository> DataTreeRepositoryList { get; } = new List<TreeRepository>();
        private DbMainEntitiesCollection _dbMainEntitiesCollection;
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
                    foreach (var item in dbRepositories)
                    {
                        DataTreeRepositoryList.Add(InfrastructureConverter.DbToBusinessRepository(item));
                    }
                }
                return DataTreeRepositoryList;
            }
        }
        public void CreateRepository(TreeRepository repository)
        {
            GetRepositoryList();

            DataTreeRepositoryList.Add(repository);
                
            using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
            {
                var result = new List<DbTreeRepository>();
                foreach (var item in DataTreeRepositoryList)
                {
                    item.DirectoryFullPath = item.DirectoryPath + Path.DirectorySeparatorChar.ToString() + item.Name;
                    if (!Directory.Exists(item.DirectoryFullPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(item.DirectoryFullPath);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    item.ConfigPath = item.DirectoryFullPath + Path.DirectorySeparatorChar.ToString() + ".repository";
                    if (!File.Exists(item.ConfigPath))
                    {
                        try
                        {
                            File.Create(item.ConfigPath);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    result.Add(InfrastructureConverter.BusinessToDbRepository(item));
                }
                xmlSerializer.Serialize(fs, result);
            }
        }
        private TreeRepository GetRepositoryContent(TreeRepository currentRepository)
        {
            _dbMainEntitiesCollection = new DbMainEntitiesCollection();
            foreach (var item in CurrentRepository.InfrastructureRepositories)
            {
                IInfrastructureRepository infrastructureRepository;
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
                        break;
                }
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
                currentRepository.ChildTreeRoots.ToList()[i] = GetRootContent(currentRepository.ChildTreeRoots.ToList()[i]);
            }
            return CurrentRepository;
        }
        private TreeRoot GetRootContent(TreeRoot treeRoot) 
        {
            IMainEntitiesRepository infrastructureRepository = InfrastructureFactory.CreateMainEntitiesRepositoriesFactory(treeRoot.InftastructureRepositoryType);
            var nodeCollection = infrastructureRepository.SelectNodes(InfrastructureConverter.BusinessToDbRepository(CurrentRepository)).Where(x => x.Guid == treeRoot.Guid);
            for (int i = 0; i < nodeCollection.Count(); i++)
            {
                //treeRoot.ChildTreeNodes.ToList()[i] = InfrastructureConverter.DbToBusinessNode(nodeCollection.ToList()[i]);
            }
            return treeRoot;
        }
        private TreeNode GetNodeContent(TreeNode treeNode)
        {
            IMainEntitiesRepository infrastructureRepository = new MongoRepository.Repositories.MainEntitуRepository();
            var nodeCollection = infrastructureRepository.SelectNodes(InfrastructureConverter.BusinessToDbRepository(CurrentRepository)).Where(x => x.Guid == treeNode.Guid);
            for (int i = 0; i < nodeCollection.Count(); i++)
            {
                treeNode.ChildTreeNodes.ToList()[i] = InfrastructureConverter.DbToBusinessNode(nodeCollection.ToList()[i]);
            }
            return treeNode;
        }
    }
}
