using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class TreeRepository : EntityBase
    {
        public List<Guid> ChildTreeRootGuids { get; set; } = new List<Guid>();
        public TreeRepository()
        {
            
        }
    }
}
