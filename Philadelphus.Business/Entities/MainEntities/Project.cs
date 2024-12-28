using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class Project
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
        public bool IsDeleted { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }
        public IEnumerable<Layer> Layers { get; set; }
        public Project()
        {
            Attributes = new List<Attribute>();
        }
    }
}
