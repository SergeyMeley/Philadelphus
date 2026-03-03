using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public abstract class MainEntityBaseVM : ViewModelBase  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        protected readonly IPhiladelphusRepositoryService _service;

        protected readonly MainEntityBaseModel _model;
        public MainEntityBaseModel Model 
        { get 
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
                        var attributeVM = new ElementAttributeVM(attribute, _service);
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
            MainEntityBaseModel mainEntityBaseModel, 
            IPhiladelphusRepositoryService service)
        {
            _service = service;

            _model = mainEntityBaseModel;
            _storageVM = new DataStorageVM(mainEntityBaseModel.DataStorage);
        }
        public ElementAttributeVM AddAttribute()
        {
            if (_model is IAttributeOwnerModel)
            {
                var attributeOwnerModel = (IAttributeOwnerModel)_model;
                var attribute = _service.CreateElementAttribute(attributeOwnerModel);
                attributeOwnerModel.AddAttribute(attribute);
                var attributeVM = new ElementAttributeVM(attribute, _service);
                AttributesVMs.Add(attributeVM);
                OnPropertyChanged(nameof(AttributesVMs));
                return attributeVM;
            }
            return null;
        }
    }
}
