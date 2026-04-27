using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies
{
    internal class EmptyPropertiesPolicy<T> : IPropertiesPolicy<T>
        where T : MainEntityBaseModel<T>
    {
        public bool CanRead(T model, string prop)
        {
            return true;
        }

        public bool CanWrite(T model, string prop, object value)
        {
            return true;
        }

        public object OnRead(T model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(T model, string prop, object oldValue, object newValue)
        {
        }
    }
}
