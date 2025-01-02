using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreesRepository : MainEntityBase
    {
        public IEnumerable<Attribute> Attributes { get; set; }
        public IEnumerable<TreeRoot> Trees { get; set; }
        public TreesRepository()
        {
            Attributes = new List<Attribute>();
            Trees = new List<TreeRoot>();
        }
    }
}
