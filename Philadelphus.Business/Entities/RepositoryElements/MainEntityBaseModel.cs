using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers.InfrastructureConverters;
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
        public IMainEntity DbEntity { get; set; }
        public abstract IDataStorageModel DataStorage { get; }
        public MainEntityBaseModel(Guid guid)
        {
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
