using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Policies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Fakes.PoliciesAndRules
{
    internal class FakeRecursivePolicy<T> : IPropertiesPolicy<T>
    where T : MainEntityBaseModel<T>
    {
        public bool CanRead(T model, string prop)
        {
            var temp = model.Uuid;
            model.Name = "Fake";
            return true;
        }

        public bool CanWrite(T model, string prop, object value)
        {
            var temp = model.Uuid;
            model.Name = "Fake";
            return true;
        }

        public object OnRead(T model, string prop, object value)
        {
            var temp = model.Uuid;
            model.Name = "Fake";
            return value;
        }

        public void OnWrite(T model, string prop, object oldValue, object newValue) 
        {
            var temp = model.Uuid;
            model.Name = "Fake";
        }
    }
}
