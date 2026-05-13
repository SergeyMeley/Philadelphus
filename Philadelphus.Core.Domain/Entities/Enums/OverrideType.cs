using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Перечисляет варианты OverrideType.
    /// </summary>
    public enum OverrideType
    {
        [Display(Name = "Ошибка системы")]
        SystemError,
        [Display(Name = "Не применимо")]
        NotApplicable,
        [Display(Name = "Запрещено")]
        Sealed,
        [Display(Name = "Возможно")] 
        Virtual,
        [Display(Name = "Требуется")] 
        Abstract
    }

    /// <summary>
    /// Представляет объект OverrideTypeItem.
    /// </summary>
    public class OverrideTypeItem
    {
        /// <summary>
        /// Отображаемое наименование.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Значение.
        /// </summary>
        public OverrideType Value { get; set; }
    }
}
