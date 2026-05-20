using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    internal class CustomCodeUniquenessStrategy<T> : ICustomCodeUniquenessStrategy<T>
        where T : WorkingTreeMemberBaseModel<T>
    {
        private readonly Func<T, IEnumerable<CustomCodeItem>> _getCustomCodeItems;

        public CustomCodeUniquenessStrategy(Func<T, IEnumerable<CustomCodeItem>> getCustomCodeItems)
        {
            _getCustomCodeItems = getCustomCodeItems;
        }

        public IEnumerable<CustomCodeItem> GetCustomCodeItems(T model)
        {
            return _getCustomCodeItems(model);
        }
    }
}
