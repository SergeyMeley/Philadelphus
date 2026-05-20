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
        /// <summary>
        /// Возвращает элементы той коллекции, в которой <c>Sequence</c> должен быть уникальным.
        /// </summary>
        /// <param name="model">Модель, для которой выполняется проверка.</param>
        /// <returns>Коллекция идентификаторов и значений Sequence для сравнения.</returns>
        IEnumerable<SequencedItem> GetSequencedItems(T model);
    }
}
