using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class DbProject : DbEntityBase
    {
        public string DirectoryPath { get; set; }
        public IEnumerable<long> AttributeIds { get; set; }
        public IEnumerable<long> LayerIds { get; set; }
        public DbProject(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
