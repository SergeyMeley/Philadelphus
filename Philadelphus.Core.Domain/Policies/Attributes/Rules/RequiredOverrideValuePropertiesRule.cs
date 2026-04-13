using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Правило, ограничивающее изменение значений атрибута, если он является собственным и абстрактным
    /// </summary>
    public class RequiredOverrideValuePropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private static readonly HashSet<string> _locked =
        [
            nameof(ElementAttributeModel.Value),
            nameof(ElementAttributeModel.Values)
        ];

        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            if (model.IsOwn && model.Override == OverrideType.Abstract)
            {
                if (_locked.Contains(prop))
                    return false;
            }

            return true;
        }

        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
