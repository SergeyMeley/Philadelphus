using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Presentation.Infrastructure;
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

        /// <summary>
        /// Все настроенные хранилища данных.
        /// </summary>
        public IEnumerable<IDataStorageModel> AllDataStorages
        {
            get
            {
                return _dataStoragesCollectionVM.DataStoragesVMs?
                    .Select(x => x.Model)
                    .OfType<IDataStorageModel>()
                    ?? Enumerable.Empty<IDataStorageModel>();
            }
        }

        public bool IsPhiladelphusRepositoryAvailable
            => OwnDataStorage.IsAvailable == true;

        /// <summary>
        /// Возможные хранилища участников кустарника по умолчанию.
        /// </summary>
        public IEnumerable<IDataStorageModel> ShrubMembersDefaultDataStorages
            => DataStorages.Where(x => x.SupportsEntityGroup(InfrastructureEntityGroups.ShrubMembers));

        /// <summary>
        /// Возможные хранилища отчетов по умолчанию.
        /// </summary>
        public IEnumerable<IDataStorageModel> ReportsDefaultDataStorages
            => DataStorages.Where(x => x.SupportsEntityGroup(InfrastructureEntityGroups.Reports));

        /// <summary>
        /// Хранилище участников кустарника по умолчанию.
        /// </summary>
        public IDataStorageModel? DefaultShrubMembersDataStorage
        {
            get => _model.DefaultShrubMembersDataStorage;
            set
            {
                if (!_model.SetDefaultShrubMembersDataStorage(value))
                    return;

                OnPropertyChanged(nameof(DefaultShrubMembersDataStorage));
                OnPropertyChanged(nameof(RemoveAvailableDataStorageCommand));
                OnPropertyChanged(nameof(ClearDefaultShrubMembersDataStorageCommand));
                NotifyStateVisibilityPropertiesChanged();
            }
        }

        /// <summary>
        /// Хранилище отчетов по умолчанию.
        /// </summary>
        public IDataStorageModel? DefaultReportsDataStorage
        {
            get => _model.DefaultReportsDataStorage;
            set
            {
                if (!_model.SetDefaultReportsDataStorage(value))
                    return;

                OnPropertyChanged(nameof(DefaultReportsDataStorage));
                OnPropertyChanged(nameof(RemoveAvailableDataStorageCommand));
                OnPropertyChanged(nameof(ClearDefaultReportsDataStorageCommand));
                NotifyStateVisibilityPropertiesChanged();
            }
        }

        /// <summary>
        /// Команда добавления возможного хранилища данных.
        /// </summary>
        public RelayCommand AddAvailableDataStorageCommand
        {
            get
            {
                return new RelayCommand(
                    storage => AddAvailableDataStorage((IDataStorageModel)storage!),
                    storage => storage is IDataStorageModel dataStorage
                        && DataStorages.All(x => x.Uuid != dataStorage.Uuid));
            }
        }

        /// <summary>
        /// Команда удаления возможного хранилища данных.
        /// </summary>
        public RelayCommand RemoveAvailableDataStorageCommand
        {
            get
            {
                return new RelayCommand(
                    storage => RemoveAvailableDataStorage((IDataStorageModel)storage!),
                    storage => storage is IDataStorageModel dataStorage
                        && dataStorage.Uuid != OwnDataStorageUuid
                        && DefaultShrubMembersDataStorage?.Uuid != dataStorage.Uuid
                        && DefaultReportsDataStorage?.Uuid != dataStorage.Uuid
                        && DataStorages.Any(x => x.Uuid == dataStorage.Uuid));
            }
        }

        /// <summary>
        /// Команда очистки хранилища участников кустарника по умолчанию.
        /// </summary>
        public RelayCommand ClearDefaultShrubMembersDataStorageCommand
        {
            get
            {
                return new RelayCommand(
                    _ => DefaultShrubMembersDataStorage = null,
                    _ => DefaultShrubMembersDataStorage != null);
            }
        }

        /// <summary>
        /// Команда очистки хранилища отчетов по умолчанию.
        /// </summary>
        public RelayCommand ClearDefaultReportsDataStorageCommand
        {
            get
            {
                return new RelayCommand(
                    _ => DefaultReportsDataStorage = null,
                    _ => DefaultReportsDataStorage != null);
            }
        }

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

            // Основное хранилище обязательно доступно репозиторию: системное дерево
            // независимо от собственного хранилища репозитория сохраняется только туда.
            var mainDataStorage = dataStoragesCollectionVM.MainDataStorageVM.Model;
            if (mainDataStorage != null
                && repositoryModel.DataStorages.All(x => x.Uuid != mainDataStorage.Uuid))
            {
                repositoryModel.AddAvailableDataStorage(mainDataStorage);
            }

            _dataStorages = new ObservableCollection<IDataStorageModel>(repositoryModel.DataStorages);
            RebuildTreeItems();
        }

        /// <summary>
        /// Добавить возможное хранилище данных.
        /// </summary>
        /// <param name="storage">Хранилище данных.</param>
        /// <returns>true, если хранилище добавлено; иначе false.</returns>
        public bool AddAvailableDataStorage(IDataStorageModel storage)
        {
            if (!_model.AddAvailableDataStorage(storage))
                return false;

            DataStorages.Add(storage);
            NotifyDataStoragesChanged();
            return true;
        }

        /// <summary>
        /// Удалить возможное хранилище данных.
        /// </summary>
        /// <param name="storage">Хранилище данных.</param>
        /// <returns>true, если хранилище удалено; иначе false.</returns>
        public bool RemoveAvailableDataStorage(IDataStorageModel storage)
        {
            if (!_model.RemoveAvailableDataStorage(storage))
                return false;

            var availableStorage = DataStorages.Single(x => x.Uuid == storage.Uuid);
            DataStorages.Remove(availableStorage);
            NotifyDataStoragesChanged();
            return true;
        }

        private void NotifyDataStoragesChanged()
        {
            OnPropertyChanged(nameof(DataStorages));
            OnPropertyChanged(nameof(ShrubMembersDefaultDataStorages));
            OnPropertyChanged(nameof(ReportsDefaultDataStorages));
            OnPropertyChanged(nameof(AddAvailableDataStorageCommand));
            OnPropertyChanged(nameof(RemoveAvailableDataStorageCommand));
            NotifyStateVisibilityPropertiesChanged();
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

            TreeItems.Add(new ShrubVM(shrub, this, _dataStoragesCollectionVM, _service, _fileDialogService, _notificationService, visibleWorkingTrees));
            OnPropertyChanged(nameof(Childs));
            OnPropertyChanged(nameof(TreeItems));
            OnPropertyChanged(nameof(ChildsCount));
        }
    }
}
