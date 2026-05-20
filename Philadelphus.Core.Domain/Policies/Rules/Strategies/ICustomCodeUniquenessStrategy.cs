using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Стратегия проверки уникальности свойства CustomCode.
    /// </summary>
    internal interface ICustomCodeUniquenessStrategy<T>
        where T : WorkingTreeMemberBaseModel<T>
    {
        /// <summary>
        /// Возвращает элементы, с которыми проверяемая модель делит пространство пользовательских кодов.
        /// </summary>
        /// <param name="model">Модель, для которой выполняется проверка.</param>
        /// <returns>Коллекция идентификаторов и CustomCode для сравнения.</returns>
        IEnumerable<CustomCodeItem> GetCustomCodeItems(T model);
    }
}
