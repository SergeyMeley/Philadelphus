using Philadelphus.Business.Entities.ElementsProperties;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.WpfApplication.ViewModels.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs.ElementsContentVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs
{
    public abstract class MainEntityBaseVM : ViewModelBase
    {
        protected readonly TreeRepositoryService _service;

        protected readonly MainEntityBaseModel _model;
        public MainEntityBaseModel Model 
        { get 
            { 
                return _model; 
            } 
        }
        public EntityTypesModel EntityType
        {
            get
            {
                return _model.EntityType;
            }
        }
        public Guid Guid 
        { 
            get
            {
                return _model.Guid;
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
        public string Alias
        {
            get
            {
                return _model.Alias;
            }
            set
            {
                _model.Alias = value;
                OnPropertyChanged(nameof(Alias));
                OnPropertyChanged(nameof(State));
            }
        }
        public string CustomCode
        {
            get
            {
                return _model.CustomCode;
            }
            set
            {
                _model.CustomCode = value;
                OnPropertyChanged(nameof(CustomCode));
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
        public EntityElementTypeModel ElementType
        {
            get
            {
                return _model.ElementType;
            }
            set
            {
                _model.ElementType = value;
                OnPropertyChanged(nameof(ElementType));
                OnPropertyChanged(nameof(State));
            }
        }
        public State State
        {
            get
            {
                return _model.State;
            }
        }
        public ObservableCollection<ElementAttributeVM> PersonalAttributesVMs { get; } = new ObservableCollection<ElementAttributeVM>();
        public ObservableCollection<ElementAttributeVM> ParentElementAttributesVMs { get; } = new ObservableCollection<ElementAttributeVM>();

        private DataStorageVM _storageVM;
        public DataStorageVM StorageVM { get => _storageVM; }
        public MainEntityBaseVM(MainEntityBaseModel mainEntityBaseModel, TreeRepositoryService service)
        {
            _service = service;
            _model = mainEntityBaseModel;
            _storageVM = new DataStorageVM(mainEntityBaseModel.DataStorage);
            if (_model is IAttributeOwnerModel)
            {
                var attributeOwnerModel = (IAttributeOwnerModel)_model;
                foreach (var attribute in attributeOwnerModel.ParentElementAttributes)
                {
                    var attributeVM = new ElementAttributeVM(attribute, _service);
                    ParentElementAttributesVMs.Add(attributeVM);
                }
                foreach (var attribute in attributeOwnerModel.PersonalAttributes)
                {
                    var attributeVM = new ElementAttributeVM(attribute, _service);
                    PersonalAttributesVMs.Add(attributeVM);
                }
            }
        }
        public ElementAttributeVM AddAttribute()
        {
            if (_model is IAttributeOwnerModel)
            {
                var attributeOwnerModel = (IAttributeOwnerModel)_model;
                var attribute = _service.CreateElementAttribute(attributeOwnerModel);
                attributeOwnerModel.PersonalAttributes.Add(attribute);
                var attributeVM = new ElementAttributeVM(attribute, _service);
                PersonalAttributesVMs.Add(attributeVM);
                OnPropertyChanged(nameof(PersonalAttributesVMs));
                return attributeVM;
            }
            return null;
        }
    }
}
