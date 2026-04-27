using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies
{
    public interface IPropertiesRule<T>
    {
        bool CanRead(ElementAttributeModel model, string prop, object value);
        bool CanWrite(ElementAttributeModel model, string prop, object value);
        object OnRead(ElementAttributeModel model, string prop, object value);
        void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue);
    }
}
