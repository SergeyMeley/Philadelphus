using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Models
{
    /// <summary>
    /// Доменная модель отчета.
    /// </summary>
    public class ReportInfoModel
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
        /// <returns>Коллекция полученных данных.</returns>
        public List<ReportParameterModel> Parameters { get; set; } = new();

        /// <summary>
        /// Список баз данных, которые требуется прикрепить для выполнения отчета.
        /// Формат: "alias1=path1;alias2=path2" или JSON
        /// </summary>
        public string AttachDatabases { get; set; }
    }
}
