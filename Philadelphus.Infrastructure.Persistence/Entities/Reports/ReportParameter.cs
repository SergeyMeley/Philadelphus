using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.Entities.Reports
{
    /// <summary>
    /// Параметр запроса
    /// </summary>
    public class ReportParameter
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Тип .NET
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Тип БД
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Обязательно к заполнению
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
