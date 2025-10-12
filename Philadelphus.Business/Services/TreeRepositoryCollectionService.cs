using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers.InfrastructureConverters;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services
{
    public class TreeRepositoryCollectionService
    {
        #region [ Props ]

        #endregion

        #region [ Construct ]

        public TreeRepositoryCollectionService()
        {
            
        }

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
                        for (int i = 0; i < repositories.Count; i++)
                        {
                            repositories[i].State = State.SavedOrLoaded;
                        }
                        result.AddRange(repositories);
                    }
                }
            }
            return result;
        }

        #endregion

        #region [ Save ]

        #endregion

        #region [ Create + Add ]

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

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

        #region [ Temp ]

        /// <summary>
        /// Создание примера репозитория.
        /// </summary>
        /// <returns></returns>
        public TreeRepositoryModel CreateSampleRepository(IDataStorageModel dataStorage)
        {
            var repo = new TreeRepositoryModel(Guid.NewGuid(), dataStorage);
            var service = new TreeRepositoryService(repo);
            service.MainEntityCollection.DataTreeRepositories.Add(repo);
            for (int i = 0; i < 5; i++)
            {
                var root = new TreeRootModel(Guid.NewGuid(), repo, dataStorage);
                service.GetAttributesSample(root);
                ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(root);
                for (int j = 0; j < 5; j++)
                {
                    var node = new TreeNodeModel(Guid.NewGuid(), root);
                    service.GetAttributesSample(node);
                    ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(node);
                    for (int k = 0; k < 5; k++)
                    {
                        var node2 = new TreeNodeModel(Guid.NewGuid(), root);
                        service.GetAttributesSample(node2);
                        repo.ElementsCollection.Add(node2);
                        node.Childs.Add(node2);
                        var leave = new TreeLeaveModel(Guid.NewGuid(), node);
                        service.GetAttributesSample(leave);
                        ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(leave);
                        node.Childs.Add(leave);
                    }
                    root.Childs.Add(node);
                }
                repo.Childs.Add(root);
            }
            return repo;
        }

        #endregion
    }
}
