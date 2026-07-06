using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Presentation.Services.StateVisibility
{
    /// <summary>
    /// Aggregated state information for the tree state indicator.
    /// </summary>
    public sealed record StateVisibilityInfo(
        State? ParentOwnerState,
        State ElementState,
        State? ChildContentState,
        string ToolTip);
}
