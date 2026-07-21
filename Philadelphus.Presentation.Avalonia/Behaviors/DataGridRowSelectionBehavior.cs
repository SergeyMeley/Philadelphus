using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.VisualTree;

namespace Philadelphus.Presentation.Avalonia.Behaviors;

/// <summary>
/// Выбирает строку DataGrid до обработки нажатия вложенными интерактивными контролами.
/// </summary>
public sealed class DataGridRowSelectionBehavior
{
    private DataGridRowSelectionBehavior()
    {
    }

    /// <summary>
    /// Включает выбор строки в туннельной фазе обработки нажатия.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<DataGridRowSelectionBehavior, DataGrid, bool>("IsEnabled");

    public static bool GetIsEnabled(DataGrid dataGrid) =>
        dataGrid.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DataGrid dataGrid, bool value) =>
        dataGrid.SetValue(IsEnabledProperty, value);

    static DataGridRowSelectionBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<DataGrid>(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(
        DataGrid dataGrid,
        AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.NewValue is true)
        {
            dataGrid.AddHandler(
                InputElement.PointerPressedEvent,
                OnPointerPressed,
                RoutingStrategies.Tunnel,
                handledEventsToo: true);
        }
        else
        {
            dataGrid.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        }
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (sender is not DataGrid dataGrid
            || eventArgs.GetCurrentPoint(dataGrid).Properties.IsLeftButtonPressed == false
            || eventArgs.Source is not Visual source)
        {
            return;
        }

        var row = source.FindAncestorOfType<DataGridRow>(includeSelf: true);
        if (row?.DataContext != null
            && ReferenceEquals(dataGrid.SelectedItem, row.DataContext) == false)
        {
            dataGrid.SelectedItem = row.DataContext;
        }
    }
}
