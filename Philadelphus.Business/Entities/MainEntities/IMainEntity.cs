using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public interface IMainEntity
    {
        public EntityTypes EntityType { get; }
        public InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public Guid Guid { get; }
        public Guid ParentGuid { get; set; }
        public long DbId { get; set; }
        public string DirectoryPath { get; set; }
        public string DirectoryFullPath { get; set; }
        public string ConfigPath { get; set; }
        public long Sequence { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool HasContent { get; set; }
        public bool IsOriginal { get; set; }
        public bool IsLegacy { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedContentOn { get; set; }
        public string UpdatedContentBy { get; set; }
        public string DeletedOn { get; set; }
        public string DeletedBy { get; set; }
    }
}
