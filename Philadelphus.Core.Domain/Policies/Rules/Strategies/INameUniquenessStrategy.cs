using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Стратегия проверки уникальности свойства Name.
    /// </summary>
    internal interface INameUniquenessStrategy<T>
        where T : MainEntityBaseModel<T>
    {
        IEnumerable<Type> ReservedPropertyTypes { get; }

        IEnumerable<NamedItem> GetNamedItems(T model);
    }
}
