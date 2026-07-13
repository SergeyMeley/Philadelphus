using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Philadelphus.Core.Domain.Relations
{
    /// <summary>
    /// Определяет вид непосредственной связи объектов репозитория.
    /// </summary>
    public enum RepositoryRelationType
    {
        [Display(Name = "Родитель")]
        Parent,
        [Display(Name = "Наследник")]
        Child,
        [Display(Name = "Содержимое")]
        Content,
        [Display(Name = "Владелец")] Owner,
        [Display(Name = "Тип данных атрибута")]
        AttributeDataType,
        [Display(Name = "Значение атрибута")]
        AttributeValue,
        [Display(Name = "Одно из значений атрибута")]
        AttributeCollectionValue,
        [Display(Name = "Ссылка из формулы")]
        FormulaReference,
        [Display(Name = "Другой тип связи")]
        Other
    }
}
