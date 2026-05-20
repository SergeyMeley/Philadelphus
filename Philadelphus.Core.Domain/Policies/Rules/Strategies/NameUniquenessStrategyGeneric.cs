using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    internal class NameUniquenessStrategy<T> : INameUniquenessStrategy<T>
        where T : MainEntityBaseModel<T>
    {
        private readonly Func<T, IEnumerable<NamedItem>> _getNamedItems;

        public NameUniquenessStrategy(
            IEnumerable<Type> reservedPropertyTypes,
            Func<T, IEnumerable<NamedItem>> getNamedItems)
        {
            ReservedPropertyTypes = reservedPropertyTypes.ToList();
            _getNamedItems = getNamedItems;
        }

        public IEnumerable<Type> ReservedPropertyTypes { get; }

        public IEnumerable<NamedItem> GetNamedItems(T model)
        {
            return _getNamedItems(model);
        }
    }
}
