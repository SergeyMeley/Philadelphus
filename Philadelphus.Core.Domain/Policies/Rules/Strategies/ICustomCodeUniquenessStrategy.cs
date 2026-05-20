using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Стратегия проверки уникальности свойства CustomCode.
    /// </summary>
    internal interface ICustomCodeUniquenessStrategy<T>
        where T : WorkingTreeMemberBaseModel<T>
    {
        IEnumerable<CustomCodeItem> GetCustomCodeItems(T model);
    }
}
