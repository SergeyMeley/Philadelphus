using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    internal class SequenceUniquenessStrategy<T> : ISequenceUniquenessStrategy<T>
        where T : MainEntityBaseModel<T>, ISequencableModel
    {
        private readonly Func<T, IEnumerable<SequencedItem>> _getSequencedItems;

        public SequenceUniquenessStrategy(Func<T, IEnumerable<SequencedItem>> getSequencedItems)
        {
            _getSequencedItems = getSequencedItems;
        }

        public IEnumerable<SequencedItem> GetSequencedItems(T model)
        {
            return _getSequencedItems(model);
        }
    }
}
