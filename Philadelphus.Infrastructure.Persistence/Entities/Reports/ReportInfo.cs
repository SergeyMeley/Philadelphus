using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.Entities.Reports
{
    /// <summary>
    /// Представляет объект описания отчета.
    /// </summary>
    public class ReportInfo
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }
       
        /// <summary>
        /// Схема.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Тип (View, MaterializedView, Function).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Параметры отчета.
        /// </summary>
        public List<ReportParameter> Parameters { get; set; } = new();

        /// <summary>
        /// Список баз данных, которые требуется прикрепить для выполнения отчета.
        /// Формат: "alias1=path1;alias2=path2" или JSON
        /// </summary>
        public string AttachDatabases { get; set; }
    }
}
