using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    public static class DataGridLostKeyboardFocusCommandBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(DataGridLostKeyboardFocusCommandBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand? GetCommand(DependencyObject obj)
        {
            return (ICommand?)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand? value)
        {
            obj.SetValue(CommandProperty, value);
        }

        private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not DataGrid dataGrid)
            {
                return;
            }

            if (e.OldValue != null)
            {
                dataGrid.LostKeyboardFocus -= OnLostKeyboardFocus;
            }

            if (e.NewValue != null)
            {
                dataGrid.LostKeyboardFocus += OnLostKeyboardFocus;
            }
        }

        private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
            {
                return;
            }

            dataGrid.Dispatcher.BeginInvoke(() =>
            {
                if (dataGrid.IsKeyboardFocusWithin)
                {
                    return;
                }

                var command = GetCommand(dataGrid);
                if (command?.CanExecute(null) == true)
                {
                    command.Execute(null);
                }
            }, DispatcherPriority.Background);
        }
    }
}
