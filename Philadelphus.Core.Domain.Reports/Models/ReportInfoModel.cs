using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Models
{
    public class ReportInfoModel
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Type { get; set; } // "View", "MaterializedView", "Function"
        public string Description { get; set; }
        public List<ReportParameterModel> Parameters { get; set; } = new();

        /// <summary>
        /// Список баз данных, которые требуется прикрепить для выполнения отчета.
        /// Формат: "alias1=path1;alias2=path2" или JSON
        /// </summary>
        public string AttachDatabases { get; set; }
    }
}
