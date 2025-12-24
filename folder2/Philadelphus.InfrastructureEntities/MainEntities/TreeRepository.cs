using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class TreeRepository
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public virtual AuditInfo AuditInfo { get; set; } = new AuditInfo();
        public Guid OwnDataStorageGuid { get; set; }
        public Guid[] DataStoragesGuids { get; set; }
        public Guid[] ChildTreeRootsGuids { get; set; }
        public TreeRepository()
        {
            
        }
    }
}
