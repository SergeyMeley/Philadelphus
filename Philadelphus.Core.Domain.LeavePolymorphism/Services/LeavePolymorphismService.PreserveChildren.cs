using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Services;

public sealed partial class LeavePolymorphismService
{
    /// <inheritdoc />
    public LeavePolymorphismPreservationResult PreserveChildrenAndResolveReplacement(
        LeavePolymorphismPropagationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        BeginPropagation();
        try
        {
            ValidatePropagationPlan(plan);

            var resolutions = new List<LeavePolymorphismResolution>();
            var createdLeaves = new List<TreeLeaveModel>();
            var directChildren = plan.Items
                .Where(x => ReferenceEquals(x.SourceLeave, plan.ChangedParentLeave))
                .Select(x => x.TargetLeave);

            foreach (var childLeave in directChildren)
            {
                var resolution = ResolveParent(childLeave);
                if (resolution.Status == LeavePolymorphismStatus.NotFound)
                {
                    // После создания первой замены следующий наследник с той же
                    // сигнатурой найдёт её через ResolveParent и переиспользует.
                    createdLeaves.AddRange(CreateParentChain(childLeave));
                    resolution = ResolveParent(childLeave);
                }

                resolutions.Add(resolution);
            }

            return new(resolutions, createdLeaves);
        }
        finally
        {
            EndPropagation();
        }
    }
}
