using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeRepositoryHeaderModel : ITreeRepositoryHeaderModel
    {
        public Guid Guid { get; protected set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public AuditInfoModel AuditInfo { get; private set; } = new AuditInfoModel();
        public IDataStorageModel OwnDataStorage { get; set; }
        public State State { get; set; } = State.Initialized;
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
        internal TreeRepositoryHeaderModel() 
        { 
        }
    }
}
