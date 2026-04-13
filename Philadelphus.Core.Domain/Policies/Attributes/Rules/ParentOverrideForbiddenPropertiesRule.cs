using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Правило, ограничивающее изменение значений свойств атрибута, для которого запрещего переопределение
    /// </summary>
    public class ParentOverrideForbiddenPropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        public bool CanWrite(ElementAttributeModel attr, string prop, object value)
        {
            if (attr.Override == OverrideType.None)
                return false;

            return true;
        }

        public object OnRead(ElementAttributeModel attr, string prop, object value)
        {
            return value;
        }

        public void OnWrite(ElementAttributeModel attr, string prop, object oldValue, object newValue) 
        { 
        }
    }
}
