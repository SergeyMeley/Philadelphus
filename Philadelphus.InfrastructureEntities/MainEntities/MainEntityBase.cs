using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public abstract class MainEntityBase : IMainEntity
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string? ParentGuid { get; set; }
        public long? Sequence { get; set; }
        public string Name { get; set; }
        public string? Alias { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool HasContent { get; set; }
        public bool IsOriginal { get; set; }
        public bool IsLegacy { get; set; }
        public AuditInfo AuditInfo { get; set; } = new AuditInfo();
        public MainEntityBase()
        {

        }
    }
}
