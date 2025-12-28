using Philadelphus.Core.Domain.Entities.ElementsProperties;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.RepositoryElements
{
    public abstract class MainEntityBaseModel : IMainEntityWritableModel, ILinkableByGuidModel
    {
        //TODO: Во всех IMainEntityModel при изменении значения свойств обновлять статус
        protected virtual string DefaultFixedPartOfName { get => "Новая основная сущность"; }
        public abstract EntityTypesModel EntityType { get; }
        public Guid Guid { get; protected set; }

        private string _name;
        public string Name 
        { 
            get
            {
                return _name;
            }
            set
            {
                _name = value;
               if(_state != State.Initialized)
                    _state = State.Changed;
            }
        }
        public string Alias { get; set; }
        public string CustomCode { get; set; }
        public string Description { get; set; }
        public bool HasAttributes { get; set; }
        public bool IsLegacy { get; set; }
        public AuditInfoModel AuditInfo { get; private set; } = new AuditInfoModel();
        public EntityElementTypeModel ElementType { get; set; }
        
        private State _state = State.Initialized;
        public State State { get => _state; }
        public IMainEntity DbEntity { get; set; }
        public abstract IDataStorageModel DataStorage { get; }
        public MainEntityBaseModel(Guid guid, IMainEntity dbEntity)
        {
            DbEntity = dbEntity;
            Guid = guid;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.AppendLine();
            sb.Append(Guid);
            return sb.ToString();
        }
        bool IMainEntityWritableModel.SetState(State newState)
        {
            _state = newState;
            return true;
        }
    }
}
