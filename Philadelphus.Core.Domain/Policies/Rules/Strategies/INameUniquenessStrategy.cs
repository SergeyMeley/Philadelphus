using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Стратегия проверки уникальности свойства Name.
    /// </summary>
    internal interface INameUniquenessStrategy<T>
        where T : MainEntityBaseModel<T>
    {
        /// <summary>
        /// Типы, имена публичных свойств которых считаются зарезервированными для проверяемой модели.
        /// </summary>
        /// <remarks>
        /// Например, для <c>TreeNodeModel</c> сюда попадают сам <c>TreeNodeModel</c>
        /// и общий базовый тип участника рабочего дерева. Это запрещает назвать пользовательский элемент
        /// так же, как системное свойство модели.
        /// </remarks>
        IEnumerable<Type> ReservedPropertyTypes { get; }

        /// <summary>
        /// Возвращает элементы, с которыми проверяемая модель делит пространство имен.
        /// </summary>
        /// <param name="model">Модель, для которой выполняется проверка.</param>
        /// <returns>Коллекция элементов с идентификатором и именем для сравнения.</returns>
        IEnumerable<NamedItem> GetNamedItems(T model);
    }
}
