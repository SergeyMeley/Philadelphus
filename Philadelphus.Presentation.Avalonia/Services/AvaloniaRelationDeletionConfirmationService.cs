using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Data;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using Philadelphus.Presentation.Models.Dialogs;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services;

/// <summary>
/// Показывает модальное предупреждение о связях, которые будут сброшены при удалении элемента.
/// </summary>
public sealed class AvaloniaRelationDeletionConfirmationService : IRelationDeletionConfirmationService
{
    /// <summary>
    /// Показывает предупреждение со списком связей, которые будут сброшены при удалении.
    /// </summary>
    /// <param name="elementDisplayName">Текстовое представление удаляемого элемента.</param>
    /// <param name="relations">Связи, которые будут сброшены.</param>
    /// <returns>true, если пользователь подтвердил удаление; иначе false.</returns>
    public Task<bool> ConfirmAsync(
        string elementDisplayName,
        IReadOnlyList<RelationDeletionWarningRow> relations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementDisplayName);
        ArgumentNullException.ThrowIfNull(relations);

        var result = false;
        var completion = new TaskCompletionSource<bool>();
        var window = CreateWindow(elementDisplayName, relations, () => result = true);
        window.Closed += (_, _) => completion.TrySetResult(result);

        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
            .Windows.LastOrDefault(x => x.IsVisible);
        if (owner == null)
            window.Show();
        else
            _ = window.ShowDialog(owner);

        return completion.Task;
    }

    /// <summary>
    /// Создаёт окно подтверждения удаления.
    /// </summary>
    /// <param name="elementDisplayName">Текстовое представление удаляемого элемента.</param>
    /// <param name="relations">Связи, которые будут сброшены.</param>
    /// <param name="confirm">Действие подтверждения удаления.</param>
    /// <returns>Созданное окно.</returns>
    private static Window CreateWindow(
        string elementDisplayName,
        IReadOnlyList<RelationDeletionWarningRow> relations,
        Action confirm)
    {
        var window = new Window
        {
            Title = "Подтверждение удаления",
            Width = 760,
            Height = 480,
            MinWidth = 600,
            MinHeight = 360,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };

        var grid = new DataGrid
        {
            ItemsSource = relations,
            IsReadOnly = true,
            AutoGenerateColumns = false,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Элемент",
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            Binding = new Binding(nameof(RelationDeletionWarningRow.Element))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Тип связи",
            Width = new DataGridLength(240),
            Binding = new Binding(nameof(RelationDeletionWarningRow.RelationType))
        });

        var yes = new Button { Content = "Всё равно удалить", MinWidth = 150, IsDefault = true };
        var no = new Button { Content = "Отмена", MinWidth = 100, IsCancel = true };
        yes.Click += (_, _) => { confirm(); window.Close(); };
        no.Click += (_, _) => window.Close();

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { yes, no }
        };
        var content = new Grid
        {
            Margin = new Thickness(20),
            RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto"),
            RowSpacing = 12
        };
        var message = new TextBlock
        {
            Text = $"Элемент {elementDisplayName} используется в {relations.Count} элементах.",
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap
        };
        var details = new TextBlock
        {
            Text = "При удалении в перечисленных элементах будут обнулены эти связи, " +
                   "что может привести к нарушению целостности данных. Всё равно удалить?",
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetRow(message, 0);
        Grid.SetRow(details, 1);
        Grid.SetRow(grid, 2);
        Grid.SetRow(buttons, 3);
        content.Children.Add(message);
        content.Children.Add(details);
        content.Children.Add(grid);
        content.Children.Add(buttons);
        window.Content = content;
        return window;
    }
}
