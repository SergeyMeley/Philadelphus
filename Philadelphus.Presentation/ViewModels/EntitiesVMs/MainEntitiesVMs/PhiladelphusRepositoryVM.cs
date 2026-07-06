using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs
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

        public string Alias
        {
            get => string.Empty;
            set { }
        }

        public string CustomCode
        {
            get => string.Empty;
            set { }
        }

        private ObservableCollection<IDataStorageModel> _dataStorages { get; }
      
        /// <summary>
        /// Хранилище данных.
        /// </summary>
        public ObservableCollection<IDataStorageModel> DataStorages { get => _dataStorages; }

        private ObservableCollection<TreeRootVM> _childs = new ObservableCollection<TreeRootVM>();
       
        /// <summary>
        /// Корни рабочих деревьев. Используется существующими командами и таблицами.
        /// </summary>
        public ObservableCollection<TreeRootVM> Childs { get => _childs; }

        private readonly ObservableCollection<ShrubVM> _treeItems = new();

        /// <summary>
        /// Верхний уровень визуального дерева обозревателя: кустарник текущего репозитория.
        /// </summary>
        public ObservableCollection<ShrubVM> TreeItems => _treeItems;

       
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
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService)
            : base(repositoryModel, dataStoragesCollectionVM, service, fileDialogService, notificationService)
        {
            ArgumentNullException.ThrowIfNull(repositoryModel.ContentShrub);
            ArgumentNullException.ThrowIfNull(repositoryModel.ContentShrub.ContentWorkingTrees);

            RebuildTreeItems();
        }

        /// <summary>
        /// Перестраивает коллекции визуального дерева из текущей доменной модели.
        /// </summary>
        public void RebuildTreeItems()
            => RebuildTreeItems(_model.ContentShrub?.ContentWorkingTrees);

        /// <summary>
        /// Перестраивает коллекции визуального дерева из указанного набора рабочих деревьев.
        /// </summary>
        public void RebuildTreeItems(IEnumerable<WorkingTreeModel>? workingTrees)
        {
            Childs.Clear();
            TreeItems.Clear();

            var shrub = _model.ContentShrub;
            if (shrub == null)
            {
                return;
            }

            var visibleWorkingTrees = (workingTrees ?? []).ToList();
            foreach (var item in visibleWorkingTrees.Select(x => x.ContentRoot))
            {
                if (item != null)
                {
                    Childs.Add(new TreeRootVM(item, _dataStoragesCollectionVM, _service, _fileDialogService, _notificationService));
                }
            }

            TreeItems.Add(new ShrubVM(shrub, _dataStoragesCollectionVM, _service, _fileDialogService, _notificationService, visibleWorkingTrees));
            OnPropertyChanged(nameof(Childs));
            OnPropertyChanged(nameof(TreeItems));
            OnPropertyChanged(nameof(ChildsCount));
        }
    }
}
