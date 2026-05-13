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
    /// Перечисляет варианты VisibilityScope.
    /// </summary>
    public enum VisibilityScope
    {
        [Display(Name = "Ошибка системы")]
        SystemError, 
        [Display(Name = "Всем")]
        Public,             
        [Display(Name = "Текущему узлу и листам")]
        Private,           
        [Display(Name = "Элементам корня")]
        Internal,         
        [Display(Name = "Наследникам")]
        Protected,         
        [Display(Name = "Элементам корня или наследникам")]
        InternalProtected 
    }

    /// <summary>
    /// Представляет объект VisibilityScopeItem.
    /// </summary>
    public class VisibilityScopeItem
    {
        /// <summary>
        /// Отображаемое наименование.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Значение.
        /// </summary>
        public VisibilityScope Value { get; set; }
    }
}
