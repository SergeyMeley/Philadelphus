using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels
{
    public abstract class MainEntityBaseVM : ViewModelBase
    {
        protected readonly TreeRepositoryService _service;

        private readonly MainEntityBaseModel _mainEntityBaseModel;
        public EntityTypesModel EntityType { get => _mainEntityBaseModel.EntityType; }
        public Guid Guid { get => _mainEntityBaseModel.Guid; }
        public string Name { get => _mainEntityBaseModel.Name; set => _mainEntityBaseModel.Name = value; }
        public string Alias { get => _mainEntityBaseModel.Alias; set => _mainEntityBaseModel.Alias = value; }
        public string CustomCode { get => _mainEntityBaseModel.CustomCode; set => _mainEntityBaseModel.CustomCode = value; }
        public string Description { get => _mainEntityBaseModel.Description; set => _mainEntityBaseModel.Description = value; }
        public bool HasContent { get => _mainEntityBaseModel.HasAttributes; }
        public bool IsOriginal { get => _mainEntityBaseModel.IsOriginal; set => _mainEntityBaseModel.IsOriginal = value; }
        public bool IsLegacy { get => _mainEntityBaseModel.IsLegacy; set => _mainEntityBaseModel.IsLegacy = value; }
        public AuditInfoModel AuditInfo { get => _mainEntityBaseModel.AuditInfo; }
        public EntityElementTypeModel ElementType { get => _mainEntityBaseModel.ElementType; set => _mainEntityBaseModel.ElementType = value; }
        public State State { get => _mainEntityBaseModel.State; }
        public MainEntityBaseVM(MainEntityBaseModel mainEntityBaseModel, TreeRepositoryService service)
        {
            _service = service;
            _mainEntityBaseModel = mainEntityBaseModel;
        }
    }
}
