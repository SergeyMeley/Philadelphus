using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Infrastructure.Persistence.MainEntities
{
    public abstract class MainEntityBase : IMainEntity
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long? Sequence { get; set; }
        public string? Alias { get; set; }
        public string? CustomCode { get; set; }
        public bool IsLegacy { get; set; }
        public AuditInfo AuditInfo { get; set; } = new AuditInfo();
        public MainEntityBase()
        {

        }
    }
}
