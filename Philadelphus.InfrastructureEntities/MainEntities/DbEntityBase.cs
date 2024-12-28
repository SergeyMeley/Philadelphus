using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public abstract class DbEntityBase : IDbEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public bool IsOriginal { get; set; }
        public string CreateOn { get; set; }
        public string CreateBy { get; set; }
        public string UpdateOn { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateContentOn { get; set; }
        public string UpdateContentBy { get; set; }
        public bool IsUndeletable { get; set; }
        public bool IsLegacy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
