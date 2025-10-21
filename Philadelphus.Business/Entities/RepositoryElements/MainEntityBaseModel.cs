using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers.InfrastructureConverters;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public abstract class MainEntityBaseModel : IMainEntityModel, ILinkableByGuidModel
    {
        protected virtual string DefaultFixedPartOfName { get => "Новая основная сущность"; }
        public abstract EntityTypesModel EntityType { get; }
        public Guid Guid { get; protected set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string CustomCode { get; set; }
        public string Description { get; set; }
        public bool HasContent { get; set; }
        public bool IsOriginal { get; set; }
        public bool IsLegacy { get; set; }
        public AuditInfoModel AuditInfo { get; private set; } = new AuditInfoModel();
        public EntityElementTypeModel ElementType { get; set; }
        public State State { get; set; } = State.Initialized;

        private IMainEntity _dbEntity;
        public IMainEntity DbEntity 
        { 
            get
            {
                if (_dbEntity == null)
                {
                    _dbEntity = this.ToDbEntity();
                }
                return _dbEntity;
            }
        }
        public abstract IDataStorageModel DataStorage { get; }
        internal MainEntityBaseModel(Guid guid, IMainEntity dbEntity = null)
        {
            _dbEntity = dbEntity;
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
    }
}
