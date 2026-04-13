using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Правило, ограничивающее создание приватного абстрактного атрибута
    /// </summary>
    public class OverrideVisibilityPropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            if (prop == nameof(ElementAttributeModel.Override) 
                && (OverrideType)value == OverrideType.Abstract)
            {
                if (model.Visibility == VisibilityScope.Private)
                    return false;
            }

            if (prop == nameof(ElementAttributeModel.Visibility) 
                && (VisibilityScope)value == VisibilityScope.Private)
            {
                if (model.Override == OverrideType.Abstract)
                    return false;
            }

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
