using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    /// <summary>
    /// Основная сущность
    /// </summary>
    /// <typeparam name="T">Модель</typeparam>
    public abstract class MainEntityBaseVM<T> : ViewModelBase, IMainEntityVM<T> where T : IMainEntityModel  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        protected readonly IPhiladelphusRepositoryService _service;
        protected readonly DataStoragesCollectionVM _dataStoragesCollectionVM;

        protected readonly T _model;
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
                OnPropertyChanged(nameof(State));
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
                OnPropertyChanged(nameof(State));
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
        public ObservableCollection<ElementAttributeVM> AttributesVMs 
        { 
            get
            {
                var result = new ObservableCollection<ElementAttributeVM>();
                if (_model is ShrubMemberBaseModel sm)
                {
                    foreach (var attribute in sm.Attributes)
                    {
                        var attributeVM = new ElementAttributeVM(attribute, _dataStoragesCollectionVM, _service);
                        result.Add(attributeVM);
                    }
                }
                return result;
            }
        }
        public ElementAttributeVM SelectedAttributeVM { get; set; }

        private DataStorageVM _storageVM;
        public DataStorageVM StorageVM { get => _storageVM; }
        public MainEntityBaseVM(
            T IMainEntityModel,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service)
        {
            _service = service ?? throw new NullReferenceException();
            _model = IMainEntityModel ?? throw new NullReferenceException();
            _dataStoragesCollectionVM = dataStoragesCollectionVM ?? throw new NullReferenceException();
            _storageVM = dataStoragesCollectionVM?.DataStoragesVMs?.SingleOrDefault(x => x.Uuid == _model.DataStorage.Uuid) ?? throw new NullReferenceException();
        }
        public ElementAttributeVM AddAttribute()
        {
            if (_model is IAttributeOwnerModel)
            {
                var attributeOwnerModel = (IAttributeOwnerModel)_model;
                var attribute = _service.CreateElementAttribute(attributeOwnerModel);
                attributeOwnerModel.AddAttribute(attribute);
                var attributeVM = new ElementAttributeVM(attribute, _dataStoragesCollectionVM, _service);
                AttributesVMs.Add(attributeVM);
                OnPropertyChanged(nameof(AttributesVMs));
                return attributeVM;
            }
            return null;
        }
    }
}
