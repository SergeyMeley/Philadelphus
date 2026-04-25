using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Models
{
    /// <summary>
    /// Параметр запроса
    /// </summary>
    public class ReportParameterModel
    {
        private object _value;

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
        /// Значение по умолчанию
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public object Value 
        { 
            get
            {
                return _value ?? DefaultValue;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Обязательно к заполнению
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
