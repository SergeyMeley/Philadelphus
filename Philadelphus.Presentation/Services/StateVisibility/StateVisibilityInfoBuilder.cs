using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;

using System.Text;

namespace Philadelphus.Presentation.Services.StateVisibility
{
    /// <summary>
    /// Builds state groups for the repository tree visibility indicator.
    /// </summary>
    public static class StateVisibilityInfoBuilder
    {
        private const string EmptyGroupText = "<пусто>";
        private const string NotApplicableGroupText = "<не применимо>";

        private static readonly State[] StatePriority =
        [
            State.SavedOrLoaded,
            State.Initialized,
            State.Changed,
            State.ForSoftDelete,
            State.SoftDeleted,
            State.ForHardDelete
        ];

        public static StateVisibilityInfo Build(IMainEntityModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var parents = GetParentStates(model);
            var owner = GetOwnerStates(model);
            var childs = GetChildStates(model);
            var content = GetContentStates(model);

            var parentOwnerState = GetHighestPriorityState(parents.States.Concat(owner.States));
            var childContentState = GetHighestPriorityState(childs.States.Concat(content.States));

            return new StateVisibilityInfo(
                parentOwnerState,
                model.State,
                childContentState,
                BuildToolTip(model.State, parents, owner, childs, content));
        }

        private static StateGroup GetParentStates(IMainEntityModel model)
        {
            if (model is not IChildrenModel child)
            {
                return StateGroup.NotApplicable();
            }

            return StateGroup.Applicable(child.AllParentsRecursive.Values.OfType<IMainEntityModel>().Select(x => x.State));
        }

        private static StateGroup GetOwnerStates(IMainEntityModel model)
        {
            if (model is not IContentModel content)
            {
                return StateGroup.NotApplicable();
            }

            return StateGroup.Applicable(content.AllOwnersRecursive.Values.OfType<IMainEntityModel>().Select(x => x.State));
        }

        private static StateGroup GetChildStates(IMainEntityModel model)
        {
            if (model is not IParentModel parent)
            {
                return StateGroup.NotApplicable();
            }

            return StateGroup.Applicable(parent.AllChildsRecursive.Values.OfType<IMainEntityModel>().Select(x => x.State));
        }

        private static StateGroup GetContentStates(IMainEntityModel model)
        {
            if (model is not IOwnerModel owner)
            {
                return StateGroup.NotApplicable();
            }

            return StateGroup.Applicable(owner.AllContentRecursive.Values.OfType<IMainEntityModel>().Select(x => x.State));
        }

        private static string BuildToolTip(
            State elementState,
            StateGroup parents,
            StateGroup owner,
            StateGroup childs,
            StateGroup content)
        {
            var builder = new StringBuilder();
            AppendGroup(builder, "Элемент", StateGroup.Applicable([elementState]));
            AppendGroup(builder, "Родители", parents);
            AppendGroup(builder, "Владелец", owner);
            AppendGroup(builder, "Наследники", childs);
            AppendGroup(builder, "Содержимое", content);
            return builder.ToString().TrimEnd();
        }

        private static void AppendGroup(StringBuilder builder, string title, StateGroup group)
        {
            builder.AppendLine($"{title}: {FormatGroup(group)}");
        }

        private static string FormatGroup(StateGroup group)
        {
            if (group.IsApplicable == false)
            {
                return NotApplicableGroupText;
            }

            var states = group.States
                .Distinct()
                .OrderBy(GetPriority)
                .Select(x => x.GetDisplayDescription())
                .ToList();

            return states.Count == 0
                ? EmptyGroupText
                : string.Join(", ", states);
        }

        private static State? GetHighestPriorityState(IEnumerable<State> states)
        {
            State? result = null;
            var resultPriority = -1;

            foreach (var state in states)
            {
                var priority = GetPriority(state);
                if (priority > resultPriority)
                {
                    result = state;
                    resultPriority = priority;
                }
            }

            return result;
        }

        private static int GetPriority(State state)
        {
            var index = Array.IndexOf(StatePriority, state);
            return index < 0 ? -1 : index;
        }

        private sealed record StateGroup(bool IsApplicable, IReadOnlyList<State> States)
        {
            public static StateGroup Applicable(IEnumerable<State> states)
                => new(true, states.ToList());

            public static StateGroup NotApplicable()
                => new(false, []);
        }
    }
}
