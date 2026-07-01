using System.Runtime.CompilerServices;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.VisualTree;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Attached behavior для схлопываемых строк/колонок Grid, изменяемых через GridSplitter.
    /// Хранит последний размер раскрытой области во View-слое и сбрасывает GridDefinition в Auto при закрытии.
    /// </summary>
    public sealed class CollapsibleGridDefinitionBehavior
    {
        private CollapsibleGridDefinitionBehavior()
        {
        }

        public enum DefinitionKind
        {
            Column,
            Row,
        }

        public static readonly AttachedProperty<Grid?> TargetGridProperty =
            AvaloniaProperty.RegisterAttached<CollapsibleGridDefinitionBehavior, Control, Grid?>("TargetGrid");

        public static readonly AttachedProperty<int> DefinitionIndexProperty =
            AvaloniaProperty.RegisterAttached<CollapsibleGridDefinitionBehavior, Control, int>("DefinitionIndex", -1);

        public static readonly AttachedProperty<DefinitionKind> KindProperty =
            AvaloniaProperty.RegisterAttached<CollapsibleGridDefinitionBehavior, Control, DefinitionKind>(
                "Kind", DefinitionKind.Column);

        public static readonly AttachedProperty<bool?> IsExpandedProperty =
            AvaloniaProperty.RegisterAttached<CollapsibleGridDefinitionBehavior, Control, bool?>("IsExpanded", true);

        public static readonly AttachedProperty<double> MinSizeProperty =
            AvaloniaProperty.RegisterAttached<CollapsibleGridDefinitionBehavior, Control, double>("MinSize");

        public static readonly AttachedProperty<double> DefaultSizeProperty =
            AvaloniaProperty.RegisterAttached<CollapsibleGridDefinitionBehavior, Control, double>("DefaultSize");

        public static readonly AttachedProperty<bool> HideControlWhenCollapsedProperty =
            AvaloniaProperty.RegisterAttached<CollapsibleGridDefinitionBehavior, Control, bool>(
                "HideControlWhenCollapsed", true);

        private static readonly ConditionalWeakTable<Control, State> States = new();

        static CollapsibleGridDefinitionBehavior()
        {
            TargetGridProperty.Changed.AddClassHandler<Control>(OnBehaviorPropertyChanged);
            DefinitionIndexProperty.Changed.AddClassHandler<Control>(OnBehaviorPropertyChanged);
            KindProperty.Changed.AddClassHandler<Control>(OnBehaviorPropertyChanged);
            IsExpandedProperty.Changed.AddClassHandler<Control>(OnBehaviorPropertyChanged);
            MinSizeProperty.Changed.AddClassHandler<Control>(OnBehaviorPropertyChanged);
            DefaultSizeProperty.Changed.AddClassHandler<Control>(OnBehaviorPropertyChanged);
            HideControlWhenCollapsedProperty.Changed.AddClassHandler<Control>(OnBehaviorPropertyChanged);
        }

        public static Grid? GetTargetGrid(Control control) => control.GetValue(TargetGridProperty);
        public static void SetTargetGrid(Control control, Grid? value) => control.SetValue(TargetGridProperty, value);

        public static int GetDefinitionIndex(Control control) => control.GetValue(DefinitionIndexProperty);
        public static void SetDefinitionIndex(Control control, int value) => control.SetValue(DefinitionIndexProperty, value);

        public static DefinitionKind GetKind(Control control) => control.GetValue(KindProperty);
        public static void SetKind(Control control, DefinitionKind value) => control.SetValue(KindProperty, value);

        public static bool? GetIsExpanded(Control control) => control.GetValue(IsExpandedProperty);
        public static void SetIsExpanded(Control control, bool? value) => control.SetValue(IsExpandedProperty, value);

        public static double GetMinSize(Control control) => control.GetValue(MinSizeProperty);
        public static void SetMinSize(Control control, double value) => control.SetValue(MinSizeProperty, value);

        public static double GetDefaultSize(Control control) => control.GetValue(DefaultSizeProperty);
        public static void SetDefaultSize(Control control, double value) => control.SetValue(DefaultSizeProperty, value);

        public static bool GetHideControlWhenCollapsed(Control control) => control.GetValue(HideControlWhenCollapsedProperty);

        public static void SetHideControlWhenCollapsed(Control control, bool value)
            => control.SetValue(HideControlWhenCollapsedProperty, value);

        private static void OnBehaviorPropertyChanged(Control control, AvaloniaPropertyChangedEventArgs e)
        {
            control.AttachedToVisualTree -= OnAttachedToVisualTree;
            control.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            control.AttachedToVisualTree += OnAttachedToVisualTree;
            control.DetachedFromVisualTree += OnDetachedFromVisualTree;

            Apply(control);
        }

        private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is Control control)
            {
                Apply(control);
            }
        }

        private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is Control control)
            {
                States.Remove(control);
            }
        }

        private static void Apply(Control control)
        {
            var grid = GetTargetGrid(control);
            var index = GetDefinitionIndex(control);
            if (grid == null || index < 0)
            {
                return;
            }

            var state = States.GetOrCreateValue(control);
            if (GetKind(control) == DefinitionKind.Row)
            {
                ApplyRow(control, grid, index, state);
            }
            else
            {
                ApplyColumn(control, grid, index, state);
            }
        }

        private static void ApplyColumn(Control control, Grid grid, int index, State state)
        {
            if (grid.ColumnDefinitions.Count <= index)
            {
                return;
            }

            var definition = grid.ColumnDefinitions[index];
            var isExpanded = GetIsExpanded(control) == true;
            control.IsVisible = isExpanded || !GetHideControlWhenCollapsed(control);

            if (isExpanded)
            {
                var minSize = GetMinSize(control);
                definition.MinWidth = minSize;
                definition.Width = new GridLength(state.LastExpandedSize ?? GetDefaultSizeOrMin(control));
                return;
            }

            state.LastExpandedSize = Max(definition.ActualWidth, GetMinSize(control));
            definition.MinWidth = 0;
            definition.Width = GridLength.Auto;
        }

        private static void ApplyRow(Control control, Grid grid, int index, State state)
        {
            if (grid.RowDefinitions.Count <= index)
            {
                return;
            }

            var definition = grid.RowDefinitions[index];
            var isExpanded = GetIsExpanded(control) == true;
            control.IsVisible = isExpanded || !GetHideControlWhenCollapsed(control);

            if (isExpanded)
            {
                var minSize = GetMinSize(control);
                definition.MinHeight = minSize;
                definition.Height = new GridLength(state.LastExpandedSize ?? GetDefaultSizeOrMin(control));
                return;
            }

            state.LastExpandedSize = Max(definition.ActualHeight, GetMinSize(control));
            definition.MinHeight = 0;
            definition.Height = GridLength.Auto;
        }

        private static double GetDefaultSizeOrMin(Control control)
        {
            var defaultSize = GetDefaultSize(control);
            return defaultSize > 0 ? defaultSize : GetMinSize(control);
        }

        private static double Max(double left, double right) => left > right ? left : right;

        private sealed class State
        {
            public double? LastExpandedSize { get; set; }
        }
    }
}
