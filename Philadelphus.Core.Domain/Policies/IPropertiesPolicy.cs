using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies
{
    public interface IPropertiesPolicy<T>
        where T : MainEntityBaseModel<T>
    {
        bool CanRead(T model, string prop);
        bool CanWrite(T model, string prop, object value);
        object OnRead(T model, string prop, object value);
        void OnWrite(T model, string prop, object oldValue, object newValue);
    }
}
