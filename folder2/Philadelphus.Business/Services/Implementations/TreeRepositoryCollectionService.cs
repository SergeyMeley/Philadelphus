using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Business.Helpers.InfrastructureConverters;
using Philadelphus.Business.Mapping;
using Philadelphus.Business.Services.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using Philadelphus.JsonRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services.Implementations
{
    public class TreeRepositoryCollectionService : ITreeRepositoryCollectionService
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly ILogger<TreeRepositoryCollectionService> _logger;
        private readonly INotificationService _notificationService;
        private readonly ITreeRepositoryService _treeRepositoryService;

        private static Dictionary<Guid, IDataStorageModel> _dataStorageModels = new Dictionary<Guid, IDataStorageModel>();
        public static Dictionary<Guid, IDataStorageModel> DataStorageModels { get => _dataStorageModels; private set => _dataStorageModels = value; }

        private static Dictionary<Guid, TreeRepositoryModel> _dataTreeRepositories = new Dictionary<Guid, TreeRepositoryModel>();
        public static Dictionary<Guid, TreeRepositoryModel> DataTreeRepositories { get => _dataTreeRepositories; private set => _dataTreeRepositories = value; }

        #endregion

        #region [ Construct ]

        public TreeRepositoryCollectionService(
            IMapper mapper,
            ILogger<TreeRepositoryCollectionService> logger,
            INotificationService notificationService,
            ITreeRepositoryService treeRepositoryService)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _treeRepositoryService = treeRepositoryService;

            _logger.LogInformation("TreeRepositoryCollectionService инициализирован.");
        }

        #endregion

        #region [ Get + Load ]

        public TreeRepository GetTreeRepositoryFromCollection(Guid guid)
        {
            return GetTreeRepositoryModelFromCollection(guid).ToDbEntity();
        }
        public List<TreeRepository> GetTreeRepositoryFromCollection(IEnumerable<Guid> guids)
        {
            return GetTreeRepositoryModelFromCollection(guids).ToDbEntityCollection();
        }
        public TreeRepositoryModel GetTreeRepositoryModelFromCollection(Guid guid)
        {
            return _dataTreeRepositories[guid];
        }
        public List<TreeRepositoryModel> GetTreeRepositoryModelFromCollection(IEnumerable<Guid> guids)
        {
            var result = new List<TreeRepositoryModel>();
            foreach (var guid in guids)
            {
                if (_dataTreeRepositories.TryGetValue(guid, out var model))
                {
                    result.Add(model);
                }
            }
            return result;
        }
        public IDataStorageModel GetStorageModelFromCollection(Guid guid)
        {
            return _dataStorageModels[guid];
        }
        public List<IDataStorageModel> GetStorageModelFromCollection(IEnumerable<Guid> guids)
        {
            var result = new List<IDataStorageModel>();
            foreach (var guid in guids)
            {
                if (_dataStorageModels.TryGetValue(guid, out var model))
                {
                    result.Add(model);
                }
            }
            return result;
        }

        /// <summary>
        /// Получение из настроечного файла коллекции заголовков репозиториев, являющихся избранными или последними запускаемыми.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeRepositoryHeaderModel> ForceLoadTreeRepositoryHeadersCollection(IDataStorageModel dataStorageModel)
        {
            if (dataStorageModel == null)
                return null;
            return dataStorageModel.TreeRepositoryHeadersCollectionInfrastructureRepository.SelectRepositoryCollection().ToModelCollection();
        }
        public IEnumerable<TreeRepositoryModel> GetTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] guids = null)
        {
            IEnumerable<TreeRepositoryModel> result = DataTreeRepositories.Values;
            if (result == null || result?.Count() == 0)
            {
                result = ForceLoadTreeRepositoriesCollection(dataStorages, guids);
            }
            return result;
        }
        public IEnumerable<TreeRepositoryModel> ForceLoadTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages, Guid[] guids = null)
        {
            if (dataStorages == null)
                return null;
            var result = new List<TreeRepositoryModel>();
            foreach (var dataStorage in dataStorages.Where(x => x.TreeRepositoriesInfrastructureRepository != null))
            {
                var infrastructure = dataStorage.TreeRepositoriesInfrastructureRepository;
                if (infrastructure.GetType().IsAssignableTo(typeof(ITreeRepositoriesInfrastructureRepository))
                    && dataStorage.IsAvailable)
                {
                    IEnumerable<TreeRepository> dbRepositories = null;
                    if (guids == null)
                        dbRepositories = infrastructure.SelectRepositories();
                    else
                        dbRepositories = infrastructure.SelectRepositories(guids);
                    //var repositories = _mapper.Map<List<TreeRepositoryModel>>(dbRepositories);
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

        public long SaveChanges(TreeRepositoryModel treeRepository)
        {
            long result = 0;
            result = _treeRepositoryService.SaveChanges(treeRepository);
            return result;
        }
        public long SaveChanges(TreeRepositoryHeaderModel treeRepositoryHeader, IDataStorageModel dataStorageModel)
        {
            if (dataStorageModel == null)
                return -1;
            long result = 0;
            var dbEntity = treeRepositoryHeader.ToDbEntity();
            result += dataStorageModel.TreeRepositoryHeadersCollectionInfrastructureRepository.UpdateRepository(dbEntity);
            return result;
        }

        #endregion

        #region [ Create + Add ]

        public IDataStorageModel CreateMainDataStorageModel(DirectoryInfo configsDirectory)
        {
            if (configsDirectory == null)
                return null;

            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder()
                .SetGeneralParameters(
                    name: "Основное хранилище настроечных файлов",
                    description: "Хранилище настроечных файлов и конфигураций в формате json-документов",
                    Guid.Empty,
                    InfrastructureTypes.JsonDocument,
                    isDisabled: false)
                .SetRepository(new JsonDataStoragesCollectionInfrastructureRepository(configsDirectory))
                .SetRepository(new JsonTreeRepositoryHeadersCollectionInfrastructureRepository(configsDirectory))
            ;
            var mainDataStorageModel = dataStorageBuilder.Build();

            _logger.LogInformation("Хранилище конфигурационных файлов инициализировано.");
            return mainDataStorageModel;
        }

        public TreeRepositoryModel CreateNewTreeRepository(IDataStorageModel dataStorage)
        {
            var result = new TreeRepositoryModel(Guid.NewGuid(), dataStorage, new TreeRepository());
            //((ITreeRepositoriesInfrastructureRepository)result.OwnDataStorage).InsertRepository(result.ToDbEntity());
            return result;
        }
        //public bool TryCreateTreeRepositoryFromHeader(ITreeRepositoryHeaderModel header, out TreeRepositoryModel outTreeRepository)
        //{
        //    var dataStorage = DataStorageModels.FirstOrDefault(x => x.Value.Guid == header.OwnDataStorageUuid).Value;
        //    var dbTreeRepository = DataTreeRepositories.FirstOrDefault(x => x.Value.Guid == header.Guid).Value.DbEntity;
        //    if (dataStorage != null && dbTreeRepository != null)
        //    {
        //        var result = new TreeRepositoryModel(header.Guid, dataStorage, dbTreeRepository);
        //        outTreeRepository = result;
        //        return true;

        //    }
        //    //((ITreeRepositoriesInfrastructureRepository)result.OwnDataStorage).InsertRepository(result.ToDbEntity());
        //    outTreeRepository = null;
        //    return false;
        //}
        public TreeRepositoryHeaderModel CreateTreeRepositoryHeaderFromTreeRepository(TreeRepositoryModel treeRepositoryModel)
        {
            var result = new TreeRepositoryHeaderModel();
            result.Guid = treeRepositoryModel.Guid;
            result.Name = treeRepositoryModel.Name;
            result.Description = treeRepositoryModel.Description;
            result.OwnDataStorageName = treeRepositoryModel.OwnDataStorageName;
            result.OwnDataStorageUuid = treeRepositoryModel.OwnDataStorageUuid;
            result.IsFavorite = treeRepositoryModel.IsFavorite;
            result.State = treeRepositoryModel.State;
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
        //public TreeRepositoryModel CreateSampleRepository(IDataStorageModel dataStorage)
        //{
        //    var repo = new TreeRepositoryModel(Guid.NewGuid(), dataStorage, new TreeRepository());
        //    DataTreeRepositories.Add(repo.Guid, repo);
        //    for (int i = 0; i < 5; i++)
        //    {
        //        var root = new TreeRootModel(Guid.NewGuid(), repo, dataStorage, new TreeRoot());
        //        _treeRepositoryService.GetAttributesSample(root);
        //        repo.ElementsCollection.Add(root);
        //        for (int j = 0; j < 5; j++)
        //        {
        //            var node = new TreeNodeModel(Guid.NewGuid(), root, new TreeNode());
        //            _treeRepositoryService.GetAttributesSample(node);
        //            repo.ElementsCollection.Add(node);
        //            for (int k = 0; k < 5; k++)
        //            {
        //                var node2 = new TreeNodeModel(Guid.NewGuid(), root, new TreeNode());
        //                _treeRepositoryService.GetAttributesSample(node2);
        //                repo.ElementsCollection.Add(node2);
        //                node.Childs.Add(node2);
        //                var leave = new TreeLeaveModel(Guid.NewGuid(), node, new TreeLeave());
        //                _treeRepositoryService.GetAttributesSample(leave);
        //                repo.ElementsCollection.Add(leave);
        //                node.Childs.Add(leave);
        //            }
        //            root.Childs.Add(node);
        //        }
        //        repo.Childs.Add(root);
        //    }
        //    return repo;
        //}

        #endregion
    }
}
