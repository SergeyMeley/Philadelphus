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

namespace Philadelphus.Business.Services
{
    public class DataTreeRepositoryService
    {
        public TreeRepository CurrentRepository { get; set; }
        public List<TreeRepository> DataTreeRepositoryList { get; } = new List<TreeRepository>();
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
            if (!DataTreeRepositoryList.Contains(repository))
            {
                DataTreeRepositoryList.Add(repository);
                
                using (var fs = new FileStream(new GeneralSettings().RepositoryListPath, FileMode.OpenOrCreate))
                {
                    try
                    {
                        var result = new List<DbTreeRepository>();
                        foreach (var item in DataTreeRepositoryList)
                        {
                            result.Add(InfrastructureConverter.BusinessToDbRepository(repository));
                        }
                        xmlSerializer.Serialize(fs, result);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
        private TreeRepository GetRepositoryContent(TreeRepository currentRepository)
        {
            CurrentRepository = DataTreeRepositoryList.Where(x => x.Name == currentRepository.Name).Last();
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
            for (int i = 0; i < treeRoot.ChildTreeNodes.Count(); i++)
            {
                //treeRoot.ChildTreeNodes.ToList()[i] = InfrastructureConverter.DbToBusinessNode(nodeCollection);
            }
            return treeRoot;
        }
    }
}
