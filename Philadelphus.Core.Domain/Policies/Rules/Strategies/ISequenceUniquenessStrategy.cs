using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Стратегия проверки уникальности свойства Sequence.
    /// </summary>
    internal interface ISequenceUniquenessStrategy<T>
        where T : MainEntityBaseModel<T>, ISequencableModel
    {
        IEnumerable<SequencedItem> GetSequencedItems(T model);
    }
}
