using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    public enum OverrideType
    {
        [Display(Name = "Не применимо")]
        NotApplicable,
        [Display(Name = "Запрещено")]
        Sealed,
        [Display(Name = "Возможно")] 
        Virtual,
        [Display(Name = "Требуется")] 
        Abstract
    }

    public class OverrideTypeItem
    {
        public string DisplayName { get; set; }
        public OverrideType Value { get; set; }
    }
}
