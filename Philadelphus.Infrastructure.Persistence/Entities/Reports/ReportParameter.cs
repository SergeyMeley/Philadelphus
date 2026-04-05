using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.Entities.Reports
{
    public class ReportParameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }      // .NET тип
        public bool IsRequired { get; set; }
        public object DefaultValue { get; set; }
        public string Description { get; set; }
    }
}
