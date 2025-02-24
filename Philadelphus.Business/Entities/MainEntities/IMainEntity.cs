using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public interface IMainEntity
    {
        public abstract EntityTypes EntityType { get; }
        public abstract InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public Guid Guid { get; }
        public Guid ParentGuid { get; }
        public long DbId { get; set; }
        public IEnumerable<IMainEntity> Childs { get; set; }
        public string DirectoryPath { get; set; }
        public string DirectoryFullPath { get; set; }
        public string ConfigPath { get; set; }
        public long Sequence { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string CustomCode { get; set; }
        public string Description { get; set; }
        public bool HasContent { get; set; }
        public bool IsOriginal { get; set; }
        public bool IsLegacy { get; set; }
        public AuditInfo AuditInfo { get; }
    }
}
