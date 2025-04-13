using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public abstract class MainEntityBase : IMainEntity, ILinkableByGuid
    {
        public abstract EntityTypes EntityType { get; }
        public Guid Guid { get; protected set; }
        //public Guid ParentGuid { get; protected set; }
        /// <summary>
        /// Идентификатор экземпляра в текущей базе данных
        /// </summary>
        public string Name { get; set; }
        public string Alias { get; set; }
        public string CustomCode { get; set; }
        public string Description { get; set; }
        public bool HasContent { get; set; }
        public bool IsOriginal { get; set; }
        public bool IsLegacy { get; set; }
        public AuditInfo AuditInfo { get; private set; }
        public EntityElementType ElementType { get; set; }
        public MainEntityBase(Guid guid)
        {
            Guid = guid;
        }
    }
}
