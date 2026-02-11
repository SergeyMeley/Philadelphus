using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

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
}
