using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRepository : MainEntityBase
    {
        public string DirectoryPath { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }
        public IEnumerable<TreeRoot> ChildTreeRoots { get; set; }
        public TreeRepository()
        {
            Attributes = new List<Attribute>();
            ChildTreeRoots = new List<TreeRoot>();
        }
    }
}
