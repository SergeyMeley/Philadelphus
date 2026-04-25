using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Policies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Fakes.PoliciesAndRules
{
    public class BlockWritePolicy<T> : IPropertiesPolicy<T>
    where T : MainEntityBaseModel<T>
    {
        public bool CanRead(T model, string prop) => true;

        public bool CanWrite(T model, string prop, object value) => false;

        public object OnRead(T model, string prop, object value) => value;

        public void OnWrite(T model, string prop, object oldValue, object newValue) { }
    }
}
