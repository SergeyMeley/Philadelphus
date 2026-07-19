using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Presentation.Models.LeavePolymorphism;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services;

/// <summary>
/// Показывает Avalonia-диалоги подтверждения полиморфных операций с сессионным чек-боксом.
/// </summary>
public sealed class AvaloniaLeavePolymorphismConfirmationService
    : ILeavePolymorphismConfirmationService
{
    private readonly LeavePolymorphismConfirmationSessionState _sessionState;

    /// <summary>
    /// Инициализирует сервис с общим состоянием текущей сессии приложения.
    /// </summary>
    /// <param name="sessionState">Сессионные настройки подтверждений.</param>
    public AvaloniaLeavePolymorphismConfirmationService(
        LeavePolymorphismConfirmationSessionState sessionState)
    {
        ArgumentNullException.ThrowIfNull(sessionState);
        _sessionState = sessionState;
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmManualFillAsync(
        IAttributeOwnerModel recipient,
        int changedAttributeCount)
    {
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentOutOfRangeException.ThrowIfNegative(changedAttributeCount);

        if (_sessionState.IsManualFillAutoConfirmed)
            return true;

        var (confirmed, remember) = await ShowAsync(
            "Заполнение по родительскому листу",
            $"Элемент-получатель: '{recipient.Name}' [{recipient.Uuid}].\n" +
            $"Будут перезаписаны значения {changedAttributeCount} атрибутов. Продолжить?");
        _sessionState.RememberManualFillDecision(confirmed, remember);
        return confirmed;
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmPropagationAsync(
        TreeLeaveModel changedParentLeave,
        int affectedLeaveCount,
        int changedAttributeCount)
    {
        ArgumentNullException.ThrowIfNull(changedParentLeave);
        ArgumentOutOfRangeException.ThrowIfNegative(affectedLeaveCount);
        ArgumentOutOfRangeException.ThrowIfNegative(changedAttributeCount);

        if (_sessionState.IsPropagationAutoConfirmed)
            return true;

        var (confirmed, remember) = await ShowAsync(
            "Обновление полиморфных наследников",
            $"Изменён родительский лист '{changedParentLeave.Name}' [{changedParentLeave.Uuid}].\n" +
            $"Будут обновлены {affectedLeaveCount} листов и {changedAttributeCount} атрибутов.\n\n" +
            "При отказе прежние значения наследников сохранятся, что может создать заменяющие родительские листы. Продолжить?");
        _sessionState.RememberPropagationDecision(confirmed, remember);
        return confirmed;
    }

    /// <summary>
    /// Показывает специализированное окно и возвращает ответ вместе с состоянием чек-бокса.
    /// </summary>
    private static Task<(bool Confirmed, bool Remember)> ShowAsync(
        string title,
        string message)
    {
        var result = false;
        // Продолжение пересчёта не должно выполняться внутри события Window.Closed:
        // иначе новое модальное действие может повторно войти в ещё закрывающееся окно.
        var completion = new TaskCompletionSource<(bool, bool)>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var remember = new CheckBox { Content = "Не показывать до конца сессии" };
        var window = CreateWindow(title, message, remember, () => result = true);
        window.Closed += (_, _) => completion.TrySetResult((result, remember.IsChecked == true));

        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
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

    /// <summary>
    /// Создаёт модальное окно подтверждения с кнопками «Да», «Нет» и сессионным чек-боксом.
    /// </summary>
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
