using System;
using System.Linq;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Philadelphus.Presentation.Models.Tables;

namespace Philadelphus.Presentation.Avalonia.Behaviors;

/// <summary>
/// Оформляет результаты поиска в таблице и прокручивает её к единственному совпадению.
/// </summary>
public sealed class DataGridSearchResultsBehavior
{
    private const string MatchCellKey = "IsSearchMatch";
    private const string MatchClass = "search-match";
    private const string MismatchClass = "search-mismatch";

    private static readonly AttachedProperty<bool> IsAttachedProperty =
        AvaloniaProperty.RegisterAttached<DataGridSearchResultsBehavior, DataGrid, bool>("IsAttached");

    /// <summary>
    /// Включает оформление и навигацию по результатам поиска.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<DataGridSearchResultsBehavior, DataGrid, bool>("IsEnabled");

    /// <summary>
    /// Единственная найденная строка, которую нужно выбрать и показать.
    /// </summary>
    public static readonly AttachedProperty<ChildCollectionTableRow?> ResolvedRowProperty =
        AvaloniaProperty.RegisterAttached<DataGridSearchResultsBehavior, DataGrid, ChildCollectionTableRow?>("ResolvedRow");

    private DataGridSearchResultsBehavior()
    {
    }

    static DataGridSearchResultsBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<DataGrid>(OnIsEnabledChanged);
        ResolvedRowProperty.Changed.AddClassHandler<DataGrid>(
            (grid, eventArgs) => UpdateSelection(grid, eventArgs.NewValue as ChildCollectionTableRow));
    }

    public static bool GetIsEnabled(DataGrid element) => element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DataGrid element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static ChildCollectionTableRow? GetResolvedRow(DataGrid element) => element.GetValue(ResolvedRowProperty);

    public static void SetResolvedRow(DataGrid element, ChildCollectionTableRow? value) =>
        element.SetValue(ResolvedRowProperty, value);

    private static void OnIsEnabledChanged(DataGrid grid, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.NewValue is true)
            Attach(grid);
        else
            Detach(grid);
    }

    private static void Attach(DataGrid grid)
    {
        if (grid.GetValue(IsAttachedProperty))
            return;

        grid.SetValue(IsAttachedProperty, true);
        grid.LoadingRow += OnLoadingRow;
        grid.PropertyChanged += OnGridPropertyChanged;
        Refresh(grid);
    }

    private static void Detach(DataGrid grid)
    {
        if (grid.GetValue(IsAttachedProperty) == false)
            return;

        grid.SetValue(IsAttachedProperty, false);
        grid.LoadingRow -= OnLoadingRow;
        grid.PropertyChanged -= OnGridPropertyChanged;
        grid.SelectedItem = null;
        foreach (var row in grid.GetVisualDescendants().OfType<DataGridRow>())
            SetSearchClasses(row, null);
    }

    private static void OnLoadingRow(object? sender, DataGridRowEventArgs eventArgs) =>
        SetSearchClasses(eventArgs.Row, eventArgs.Row.DataContext as ChildCollectionTableRow);

    private static void OnGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (sender is DataGrid grid && eventArgs.Property == DataGrid.ItemsSourceProperty)
            Refresh(grid);
    }

    private static void Refresh(DataGrid grid)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var row in grid.GetVisualDescendants().OfType<DataGridRow>())
                SetSearchClasses(row, row.DataContext as ChildCollectionTableRow);
            UpdateSelection(grid, GetResolvedRow(grid));
        }, DispatcherPriority.Loaded);
    }

    private static void SetSearchClasses(DataGridRow row, ChildCollectionTableRow? item)
    {
        var match = item?[MatchCellKey];
        row.Classes.Set(MatchClass, match is true);
        row.Classes.Set(MismatchClass, match is false);
    }

    private static void UpdateSelection(DataGrid grid, ChildCollectionTableRow? row)
    {
        if (GetIsEnabled(grid) == false)
            return;

        grid.SelectedItem = row;
        if (row == null)
            return;

        grid.ScrollIntoView(row, null);
        Dispatcher.UIThread.Post(
            () => grid.ScrollIntoView(row, null),
            DispatcherPriority.Loaded);
    }
}
