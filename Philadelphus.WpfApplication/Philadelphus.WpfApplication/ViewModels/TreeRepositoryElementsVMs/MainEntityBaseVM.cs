using Philadelphus.Business.Entities.ElementsProperties;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.ViewModels.TreeRepositoryElementsVMs.ElementsContentVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels
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
        public EntityTypesModel EntityType { get => _model.EntityType; }
        public Guid Guid { get => _model.Guid; }
        public string Name { get => _model.Name; set => _model.Name = value; }
        public string Alias { get => _model.Alias; set => _model.Alias = value; }
        public string CustomCode { get => _model.CustomCode; set => _model.CustomCode = value; }
        public string Description { get => _model.Description; set => _model.Description = value; }
        public AuditInfoModel AuditInfo { get => _model.AuditInfo; }
        public EntityElementTypeModel ElementType { get => _model.ElementType; set => _model.ElementType = value; }
        public State State { get => _model.State; }
        public ObservableCollection<ElementAttributeVM> PersonalAttributesVMs { get; set; } = new ObservableCollection<ElementAttributeVM>();
        public ObservableCollection<ElementAttributeVM> ParentElementAttributesVMs { get; set; } = new ObservableCollection<ElementAttributeVM>();

        public MainEntityBaseVM(MainEntityBaseModel mainEntityBaseModel, TreeRepositoryService service)
        {
            _service = service;
            _model = mainEntityBaseModel;
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
