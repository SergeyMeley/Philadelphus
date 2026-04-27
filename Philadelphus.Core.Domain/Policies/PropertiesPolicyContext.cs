using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies
{
    internal class PropertiesPolicyContext
    {
        private readonly HashSet<(object, object, string)> _readingProps = new();

        public bool Enter(object model, object field, string prop)
        {
            return _readingProps.Add((model, field, prop));
        }

        public void Exit(object model, object field, string prop)
        {
            _readingProps.Remove((model, field, prop));
        }

        public bool IsInProgress(object model, object field, string prop)
        {
            return _readingProps.Contains((model, field, prop));
        }
    }
}
