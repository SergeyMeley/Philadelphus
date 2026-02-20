using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    public enum VisibilityScope
    {
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

    public class VisibilityScopeItem
    {
        public string DisplayName { get; set; }
        public VisibilityScope Value { get; set; }
    }
}
