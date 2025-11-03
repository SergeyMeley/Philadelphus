using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers.InfrastructureConverters;
using Philadelphus.Business.Mapping;
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

namespace Philadelphus.Business.Services
{
    public class TreeRepositoryCollectionService
    {
        #region [ Props ]

        private readonly IMapper _mapper;

        private static Dictionary<Guid, IDataStorageModel> _dataStorageModels = new Dictionary<Guid, IDataStorageModel>();
        public static Dictionary<Guid, IDataStorageModel> DataStorageModels { get => _dataStorageModels; private set => _dataStorageModels = value; }

        private static Dictionary<Guid, TreeRepositoryModel> _dataTreeRepositories = new Dictionary<Guid, TreeRepositoryModel>();
        public static Dictionary<Guid, TreeRepositoryModel> DataTreeRepositories { get => _dataTreeRepositories; private set => _dataTreeRepositories = value; }

        private static IDataStorageModel _mainDataStorageModel;

        #endregion

        #region [ Construct ]

        public TreeRepositoryCollectionService(DirectoryInfo configsDirectory)
        {
            //TODO: ВРЕСЕННО, ПЕРЕДЕЛАТЬ ПОЛНОСТЬЮ КОНСТРУКТОР, МАППЕР ПОЛУЧИТЬ ИЗ DI-КОНЕТЕЙНЕРА
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Philadelphus", LogLevel.Debug)
                    ;
            });

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                // Добавьте другие профили здесь
            }, loggerFactory);

            _mapper = configuration.CreateMapper();
            DataStorageBuilder dataStorageBuilder = new DataStorageBuilder();
            dataStorageBuilder
                .SetGeneralParameters(name: "Основное хранилище", description: "Хранилище настроечных файлов в формате Json", Guid.Empty, InfrastructureTypes.JsonDocument, isDisabled: false)
                .SetRepository(new JsonTreeRepositoryHeadersCollectionInfrastructureRepository(configsDirectory));
            _mainDataStorageModel = dataStorageBuilder.Build();
        }

        #endregion

        #region [ Get + Load ]

        public static TreeRepository GetTreeRepositoryFromCollection(Guid guid)
        {
            return GetTreeRepositoryModelFromCollection(guid).ToDbEntity();
        }
        public static List<TreeRepository> GetTreeRepositoryFromCollection(IEnumerable<Guid> guids)
        {
            return GetTreeRepositoryModelFromCollection(guids).ToDbEntityCollection();
        }
        public static TreeRepositoryModel GetTreeRepositoryModelFromCollection(Guid guid)
        {
            return _dataTreeRepositories[guid];
        }
        public static List<TreeRepositoryModel> GetTreeRepositoryModelFromCollection(IEnumerable<Guid> guids)
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
        public static IDataStorageModel GetStorageModelFromCollection(Guid guid)
        {
            return _dataStorageModels[guid];
        }
        public static List<IDataStorageModel> GetStorageModelFromCollection(IEnumerable<Guid> guids)
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
        public IEnumerable<TreeRepositoryHeaderModel> GetTreeRepositoryHeadersCollection()
        {
            return _mainDataStorageModel.TreeRepositoryHeadersCollectionInfrastructureRepository.SelectRepositoryCollection().ToModelCollection();
        }
        public IEnumerable<TreeRepositoryModel> LoadTreeRepositoriesCollection(IEnumerable<IDataStorageModel> dataStorages)
        {
            var result = new List<TreeRepositoryModel>();
            foreach (var dataStorage in dataStorages)
            {
                var infrastructure = dataStorage.TreeRepositoriesInfrastructureRepository;
                if (infrastructure.GetType().IsAssignableTo(typeof(ITreeRepositoriesInfrastructureRepository))
                    && dataStorage.IsAvailable)
                {
                    var dbRepositories = infrastructure.SelectRepositories();
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
            var service = new TreeRepositoryService(treeRepository);
            long result = 0;
            result = service.SaveChanges(treeRepository);
            return result;
        }
        public long SaveChanges(TreeRepositoryHeaderModel treeRepositoryHeader)
        {
            long result = 0;
            var dbEntity = treeRepositoryHeader.ToDbEntity();
            result += _mainDataStorageModel.TreeRepositoryHeadersCollectionInfrastructureRepository.UpdateRepository(dbEntity);
            return result;
        }

        #endregion

        #region [ Create + Add ]

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
        public TreeRepositoryModel CreateSampleRepository(IDataStorageModel dataStorage)
        {
            var repo = new TreeRepositoryModel(Guid.NewGuid(), dataStorage, new TreeRepository());
            var service = new TreeRepositoryService(repo);
            TreeRepositoryCollectionService.DataTreeRepositories.Add(repo.Guid, repo);
            for (int i = 0; i < 5; i++)
            {
                var root = new TreeRootModel(Guid.NewGuid(), repo, dataStorage, new TreeRoot());
                service.GetAttributesSample(root);
                ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(root);
                for (int j = 0; j < 5; j++)
                {
                    var node = new TreeNodeModel(Guid.NewGuid(), root, new TreeNode());
                    service.GetAttributesSample(node);
                    ((List<TreeRepositoryMemberBaseModel>)repo.ElementsCollection).Add(node);
                    for (int k = 0; k < 5; k++)
                    {
                        var node2 = new TreeNodeModel(Guid.NewGuid(), root, new TreeNode());
                        service.GetAttributesSample(node2);
                        repo.ElementsCollection.Add(node2);
                        node.Childs.Add(node2);
                        var leave = new TreeLeaveModel(Guid.NewGuid(), node, new TreeLeave());
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
