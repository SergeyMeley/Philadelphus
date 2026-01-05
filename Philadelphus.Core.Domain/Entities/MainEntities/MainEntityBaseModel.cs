using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Text;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    public abstract class MainEntityBaseModel : IMainEntityWritableModel, ILinkableByUuidModel
    {
        //TODO: Во всех IMainEntityModel при изменении значения свойств обновлять статус
        protected virtual string DefaultFixedPartOfName { get => "Новая основная сущность"; }
        public abstract EntityTypesModel EntityType { get; }
        public Guid Uuid { get; protected set; }

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
        public MainEntityBaseModel(Guid uuid, IMainEntity dbEntity)
        {
            DbEntity = dbEntity;
            Uuid = uuid;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.AppendLine();
            sb.Append(Uuid);
            return sb.ToString();
        }
        bool IMainEntityWritableModel.SetState(State newState)
        {
            _state = newState;
            return true;
        }
    }
}
