using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    /// <summary>
    /// Модель представления для репозитория Чубушника.
    /// </summary>
    public class PhiladelphusRepositoryVM : MainEntityBaseVM<PhiladelphusRepositoryModel>
    {
        public IDataStorageModel OwnDataStorage 
        {
            get
            {
                return _model.OwnDataStorage;
            }
        }

        private DataStorageVM _storageVM;
       
        /// <summary>
        /// Модель представления хранилища данных.
        /// </summary>
        public DataStorageVM StorageVM { get => _storageVM; }
        public string OwnDataStorageName
        {
            get
            {
                return _model.OwnDataStorageName;
            }
        }
        public Guid OwnDataStorageUuid
        {
            get
            {
                return _model.OwnDataStorageUuid;
            }
        }

        private ObservableCollection<IDataStorageModel> _dataStorages { get; }
      
        /// <summary>
        /// Хранилище данных.
        /// </summary>
        public ObservableCollection<IDataStorageModel> DataStorages { get => _dataStorages; }

        private ObservableCollection<TreeRootVM> _childs = new ObservableCollection<TreeRootVM>();
       
        /// <summary>
        /// Дочерние элементы.
        /// </summary>
        public ObservableCollection<TreeRootVM> Childs { get => _childs; }

       
        /// <summary>
        /// Выполняет операцию ChildsCount.
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public string ChildsCount { get => $"Детей: {Childs?.Count()}, Корней: {_model?.ContentShrub?.ContentWorkingTrees?.Count()}, Uuids: NOT IMPLEMENTED"; }

        public bool IsFavorite
        {
            get
            {
                return _model.IsFavorite;
            }
            set
            {
                _model.IsFavorite = value;
            }
        }

        public DateTime? LastOpening
        {
            get
            {
                return _model.LastOpening;
            }
            set
            {
                _model.LastOpening = value;
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PhiladelphusRepositoryVM" />.
        /// </summary>
        /// <param name="repositoryModel">Параметр repositoryModel.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public PhiladelphusRepositoryVM(
            PhiladelphusRepositoryModel repositoryModel,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service)
            : base(repositoryModel, dataStoragesCollectionVM, service)
        {
            ArgumentNullException.ThrowIfNull(repositoryModel.ContentShrub);
            ArgumentNullException.ThrowIfNull(repositoryModel.ContentShrub.ContentWorkingTrees);

            foreach (var item in repositoryModel.ContentShrub.ContentWorkingTrees.Select(x => x.ContentRoot))
            {
                Childs.Add(new TreeRootVM(item, _dataStoragesCollectionVM, service));
            }
        }
    }
}
