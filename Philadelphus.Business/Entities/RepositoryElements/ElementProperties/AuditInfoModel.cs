using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.ElementProperties
{
    public class AuditInfoModel
    {
        public bool IsDeleted { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedContentOn { get; set; }
        public string UpdatedContentBy { get; set; }
        public string DeletedOn { get; set; }
        public string DeletedBy { get; set; }
    }
}
