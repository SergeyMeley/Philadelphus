using Avalonia;
using Avalonia.Controls;

using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Presentation.Avalonia.Controls
{
    /// <summary>
    /// Единая узкая полоса комбинированного статуса: владельцы/родители, сам элемент, наследники/содержимое.
    /// </summary>
    public partial class StateVisibilityStripe : UserControl
    {
        public static readonly StyledProperty<State> ParentOwnerStateProperty =
            AvaloniaProperty.Register<StateVisibilityStripe, State>(
                nameof(ParentOwnerState),
                State.SavedOrLoaded);

        public static readonly StyledProperty<State> StateProperty =
            AvaloniaProperty.Register<StateVisibilityStripe, State>(
                nameof(State),
                State.SavedOrLoaded);

        public static readonly StyledProperty<State> ChildContentStateProperty =
            AvaloniaProperty.Register<StateVisibilityStripe, State>(
                nameof(ChildContentState),
                State.SavedOrLoaded);

        public static readonly StyledProperty<string?> StateVisibilityToolTipProperty =
            AvaloniaProperty.Register<StateVisibilityStripe, string?>(
                nameof(StateVisibilityToolTip));

        public StateVisibilityStripe()
        {
            InitializeComponent();
        }

        public State ParentOwnerState
        {
            get => GetValue(ParentOwnerStateProperty);
            set => SetValue(ParentOwnerStateProperty, value);
        }

        public State State
        {
            get => GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public State ChildContentState
        {
            get => GetValue(ChildContentStateProperty);
            set => SetValue(ChildContentStateProperty, value);
        }

        public string? StateVisibilityToolTip
        {
            get => GetValue(StateVisibilityToolTipProperty);
            set => SetValue(StateVisibilityToolTipProperty, value);
        }
    }
}
