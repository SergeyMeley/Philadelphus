using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Helpers;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Services.StateVisibility;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    /// <summary>
    /// Основная сущность
    /// </summary>
    /// <typeparam name="T">Модель</typeparam>
    public abstract class MainEntityBaseVM<T> : ViewModelBase, IMainEntityVM<T> where T : IMainEntityModel  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        protected readonly IPhiladelphusRepositoryService _service;
        protected readonly IFileDialogService? _fileDialogService;
        protected readonly INotificationService? _notificationService;

        protected readonly DataStoragesCollectionVM _dataStoragesCollectionVM;

        protected readonly T _model;
        private bool? _isTreeExpanded;
        private readonly ObservableCollection<ElementAttributeVM> _attributesVMs = new();
        private ElementAttributeVM? _selectedAttributeVM;
        public T Model 
        { 
            get 
            { 
                return _model; 
            } 
        }
        public Guid Uuid 
        { 
            get
            {
                return _model.Uuid;
            }
        }
        public string Name
        {
            get
            {
                return _model.Name;
            }
            set
            {
                _model.Name = value;
                OnPropertyChanged(nameof(Name));
                NotifyStateVisibilityPropertiesChanged();
            }
        }
        
        public string Description
        {
            get
            {
                return _model.Description;
            }
            set
            {
                _model.Description = value;
                OnPropertyChanged(nameof(Description));
                NotifyStateVisibilityPropertiesChanged();
            }
        }
        public AuditInfoModel AuditInfo 
        {
            get
            {
                return _model.AuditInfo;
            }
        }
        public State State
        {
            get
            {
                return _model.State;
            }
        }
        public bool IsHidden
        {
            get
            {
                return _model.IsHidden;
            }
            set
            {
                _model.IsHidden = value;
                OnPropertyChanged(nameof(IsHidden));
                OnPropertyChanged(nameof(HeaderOpacity));
            }
        }

        public double HeaderOpacity
        {
            get
            {
                return HiddenElementPresentationHelper.GetOpacity(IsHidden);
            }
        }

        public string EntityType => _model.GetType().Name;

        public State ParentOwnerAggregateState => StateVisibilityInfoBuilder.Build(_model).ParentOwnerState ?? State.SavedOrLoaded;

        public State ChildContentAggregateState => StateVisibilityInfoBuilder.Build(_model).ChildContentState ?? State.SavedOrLoaded;

        public string StateVisibilityToolTip => StateVisibilityInfoBuilder.Build(_model).ToolTip;

        /// <summary>
        /// Дочерние элементы для визуального дерева обозревателя.
        /// </summary>
        public virtual IEnumerable<IMainEntityVM> TreeChilds => Array.Empty<IMainEntityVM>();

        /// <summary>
        /// Нужно ли раскрыть элемент в TreeView при первичном отображении.
        /// </summary>
        public virtual bool IsTreeExpandedByDefault => false;

        /// <summary>
        /// Признак раскрытия элемента в дереве обозревателя репозитория.
        /// </summary>
        public bool IsTreeExpanded
        {
            get => _isTreeExpanded ?? IsTreeExpandedByDefault;
            set => SetProperty(ref _isTreeExpanded, value);
        }

        public long Sequence 
        { 
            get
            {
                if (_model is ISequencableModel s)
                {
                    return s.Sequence;
                }
                return -1;
            }
            set
            {
                if (_model is ISequencableModel s)
                {
                    s.Sequence = value;
                }
            }
        }
        public ObservableCollection<ElementAttributeVM> AttributesVMs
        {
            get
            {
                SynchronizeAttributesVMs();
                return _attributesVMs;
            }
        }

        /// <summary>
        /// Выбранная модель представления атрибута.
        /// </summary>
        public ElementAttributeVM? SelectedAttributeVM
        {
            get => _selectedAttributeVM;
            set => SetProperty(ref _selectedAttributeVM, value);
        }

        private DataStorageVM _storageVM;

        /// <summary>
        /// Модель представления хранилища данных.
        /// </summary>
        public DataStorageVM StorageVM { get => _storageVM; }
      
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainEntityBaseVM" />.
        /// </summary>
        /// <param name="IMainEntityModel">Параметр IMainEntityModel.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public MainEntityBaseVM(
            T IMainEntityModel,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(fileDialogService);
            ArgumentNullException.ThrowIfNull(IMainEntityModel);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM.DataStoragesVMs);
            ArgumentNullException.ThrowIfNull(IMainEntityModel.DataStorage);
            ArgumentNullException.ThrowIfNull(notificationService);

            _service = service;
            _fileDialogService = fileDialogService;
            _notificationService = notificationService;

            _model = IMainEntityModel;
            _dataStoragesCollectionVM = dataStoragesCollectionVM;
            _storageVM = dataStoragesCollectionVM?.DataStoragesVMs?.SingleOrDefault(x => x.Uuid == _model.DataStorage.Uuid) ?? throw new NullReferenceException();

            SynchronizeAttributesVMs();
        }
       
        /// <summary>
        /// Добавляет данные AddAttribute.
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public ElementAttributeVM AddAttribute()
        {
            if (_model is IAttributeOwnerModel)
            {
                var attributeOwnerModel = (IAttributeOwnerModel)_model;
                var attribute = _service.CreateElementAttribute(attributeOwnerModel);
                if (attribute != null)
                {
                    attributeOwnerModel.AddAttribute(attribute);
                    SynchronizeAttributesVMs();
                    var attributeVM = _attributesVMs.Single(x => x.Uuid == attribute.Uuid);
                    OnPropertyChanged(nameof(AttributesVMs));
                    NotifyStateVisibilityPropertiesChanged();
                    return attributeVM;
                }
            }
            return null;
        }

        /// <summary>
        /// Синхронизирует стабильную UI-коллекцию атрибутов с актуальным доменным списком.
        /// Существующие модели представления сохраняются, чтобы не сбрасывать выделение и состояние редакторов.
        /// </summary>
        private void SynchronizeAttributesVMs()
        {
            if (_model is not IShrubMemberModel shrubMember)
            {
                return;
            }

            var attributes = shrubMember.Attributes;
            var actualUuids = attributes.Select(x => x.Uuid).ToHashSet();

            for (var index = _attributesVMs.Count - 1; index >= 0; index--)
            {
                if (actualUuids.Contains(_attributesVMs[index].Uuid) == false)
                {
                    _attributesVMs.RemoveAt(index);
                }
            }

            for (var targetIndex = 0; targetIndex < attributes.Count; targetIndex++)
            {
                var attribute = attributes[targetIndex];
                var currentIndex = -1;
                for (var index = 0; index < _attributesVMs.Count; index++)
                {
                    if (_attributesVMs[index].Uuid == attribute.Uuid)
                    {
                        currentIndex = index;
                        break;
                    }
                }

                if (currentIndex < 0)
                {
                    _attributesVMs.Insert(targetIndex, new ElementAttributeVM(
                        attribute,
                        _dataStoragesCollectionVM,
                        _service,
                        fileDialogService: _fileDialogService,
                        notificationService: _notificationService));
                }
                else if (currentIndex != targetIndex)
                {
                    _attributesVMs.Move(currentIndex, targetIndex);
                }
            }
        }

        protected void NotifyStateVisibilityPropertiesChanged()
        {
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(ParentOwnerAggregateState));
            OnPropertyChanged(nameof(ChildContentAggregateState));
            OnPropertyChanged(nameof(StateVisibilityToolTip));
        }
    }
}
