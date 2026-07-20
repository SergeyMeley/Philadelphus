using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services;

/// <summary>
/// Показывает Avalonia-диалог добавления созданного значения в массив.
/// </summary>
public sealed class AvaloniaAttributeValueCreationConfirmationService
    : IAttributeValueCreationConfirmationService
{
    private bool _isAutoConfirmed;

    /// <inheritdoc />
    public async Task<bool> ConfirmAdditionAsync(
        TreeLeaveModel createdLeave,
        ElementAttributeModel attribute)
    {
        ArgumentNullException.ThrowIfNull(createdLeave);
        ArgumentNullException.ThrowIfNull(attribute);

        if (_isAutoConfirmed)
            return true;

        var (confirmed, remember) = await ShowAsync(
            "Добавление значения в массив",
            $"Создан лист '{createdLeave.Name}' [{createdLeave.Uuid}].\n"
            + $"Добавить его в массив атрибута '{attribute.Name}'?");
        if (confirmed && remember)
            _isAutoConfirmed = true;

        return confirmed;
    }

    private static Task<(bool Confirmed, bool Remember)> ShowAsync(
        string title,
        string message)
    {
        var result = false;
        var completion = new TaskCompletionSource<(bool, bool)>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var remember = new CheckBox
        {
            Content = "Не показывать до конца сессии"
        };
        var window = CreateWindow(title, message, remember, () => result = true);
        window.Closed += (_, _) =>
            completion.TrySetResult((result, remember.IsChecked == true));

        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?
            .Windows.LastOrDefault(x => x.IsVisible);
        if (owner == null)
            window.Show();
        else
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _ = window.ShowDialog(owner);
        }

        return completion.Task;
    }

    private static Window CreateWindow(
        string title,
        string message,
        CheckBox remember,
        Action confirm)
    {
        var window = new Window
        {
            Title = title,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            MinWidth = 420,
            MaxWidth = 620,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        var yes = new Button { Content = "Да", MinWidth = 90, IsDefault = true };
        var no = new Button { Content = "Нет", MinWidth = 90, IsCancel = true };
        yes.Click += (_, _) => { confirm(); window.Close(); };
        no.Click += (_, _) => window.Close();
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { yes, no }
        };
        window.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 14,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                remember,
                buttons
            }
        };
        return window;
    }
}
