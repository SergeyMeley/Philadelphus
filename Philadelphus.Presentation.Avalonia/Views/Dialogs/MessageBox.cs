using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;

namespace Philadelphus.Presentation.Avalonia.Views.Dialogs
{
    /// <summary>
    /// Минимальное программно построенное модальное окно сообщения/подтверждения.
    /// В Avalonia нет встроенного MessageBox; это лёгкая замена WPF MessageBox.Show
    /// для платформенных диалоговых сервисов.
    /// </summary>
    internal static class MessageBox
    {
        public static Task ShowAsync(Window? owner, string title, string message)
            => ShowCore(owner, title, message, confirm: false);

        public static Task<bool> ConfirmAsync(Window? owner, string title, string message)
            => ShowCore(owner, title, message, confirm: true);

        private static Task<bool> ShowCore(Window? owner, string title, string message, bool confirm)
        {
            var result = false;
            var tcs = new TaskCompletionSource<bool>();

            var window = new Window
            {
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight,
                CanResize = false,
                WindowStartupLocation = owner is null
                    ? WindowStartupLocation.CenterScreen
                    : WindowStartupLocation.CenterOwner,
                MinWidth = 320,
                MaxWidth = 560,
                ShowInTaskbar = false
            };

            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 18)
            };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };

            if (confirm)
            {
                var yes = new Button { Content = "Да", MinWidth = 90, IsDefault = true };
                var no = new Button { Content = "Нет", MinWidth = 90, IsCancel = true };
                yes.Click += (_, _) => { result = true; window.Close(); };
                no.Click += (_, _) => { result = false; window.Close(); };
                buttons.Children.Add(yes);
                buttons.Children.Add(no);
            }
            else
            {
                var ok = new Button { Content = "ОК", MinWidth = 90, IsDefault = true, IsCancel = true };
                ok.Click += (_, _) => { result = true; window.Close(); };
                buttons.Children.Add(ok);
            }

            window.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Children = { messageText, buttons }
            };

            window.Closed += (_, _) => tcs.TrySetResult(result);

            if (owner is not null)
            {
                _ = window.ShowDialog(owner);
            }
            else
            {
                window.Show();
            }

            return tcs.Task;
        }
    }
}
