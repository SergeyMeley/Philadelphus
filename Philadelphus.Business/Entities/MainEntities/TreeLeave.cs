using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeLeave : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Leave; }
        public string Uuid { get; set; }
        public string Type { get; set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public TreeLeave(string name, long id) : base(name, id)
        {
            
        }
        public TreeLeave(string name, string path) : base(name, path)
        {
            
        }
    }
}
