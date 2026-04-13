using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;

namespace Philadelphus.Core.Domain.Policies.Attributes
{
    internal class CompositeAttributePropertiesPolicy : IAttributePropertiesPolicy
    {
        private readonly List<IAttributePropertiesRule<ElementAttributeModel>> _rules;

        public CompositeAttributePropertiesPolicy(IEnumerable<IAttributePropertiesRule<ElementAttributeModel>> rules)
        {
            _rules = rules.ToList();
        }

        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return _rules.All(r => r.CanRead(model, prop));
        }

        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            return _rules.All(r => r.CanWrite(model, prop, value));
        }

        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            foreach (var r in _rules)
                value = r.OnRead(model, prop, value);

            return value;
        }

        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
            foreach (var r in _rules)
                r.OnWrite(model, prop, oldValue, newValue);
        }
    }
}
