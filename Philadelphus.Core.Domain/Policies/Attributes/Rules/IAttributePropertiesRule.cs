using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Задает контракт для правила свойств атрибутов.
    /// </summary>
    public interface IAttributePropertiesRule<T>
    {
        bool CanRead(T model, string prop);
        bool CanWrite(T model, string prop, object value);
        object OnRead(T model, string prop, object value);
        void OnWrite(T model, string prop, object oldValue, object newValue);
    }
}
