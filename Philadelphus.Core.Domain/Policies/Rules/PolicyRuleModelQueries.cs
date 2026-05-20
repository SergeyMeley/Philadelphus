using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Общие запросы к моделям для правил свойств.
    /// </summary>
    internal static class PolicyRuleModelQueries
    {
        public static IEnumerable<ElementAttributeModel> GetDirectAttributes(IOwnerModel owner)
        {
            if (owner is IWorkingTreeMemberModel workingTreeMember)
            {
                return workingTreeMember.OwningWorkingTree.ContentAttributes
                    .Where(x => x.Owner?.Uuid == owner.Uuid);
            }

            return owner.Content.Values.OfType<ElementAttributeModel>();
        }
    }
}
