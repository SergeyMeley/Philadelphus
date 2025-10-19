using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    public interface ITreeRepositoryHeaderModel
    {
        public Guid Guid { get; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public AuditInfoModel AuditInfo { get; }
        public IDataStorageModel OwnDataStorage { get; }
        public State State { get; }
        public DateTime? LastOpening { get; }
        public bool IsFavorite { get; }
    }
}
