using System.Windows.Input;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Выполняет команду после окончательного ухода фокуса из DataGrid.
    /// Avalonia-аналог WPF DataGridLostKeyboardFocusCommandBehavior: используется для отложенной
    /// пересортировки таблицы наследников (редактирование Sequence не дергает порядок во время ввода).
    /// </summary>
    public class DataGridLostKeyboardFocusCommandBehavior
    {
        private DataGridLostKeyboardFocusCommandBehavior()
        {
        }

        /// <summary>Команда, выполняемая после потери фокуса DataGrid.</summary>
        public static readonly AttachedProperty<ICommand?> CommandProperty =
            AvaloniaProperty.RegisterAttached<DataGridLostKeyboardFocusCommandBehavior, DataGrid, ICommand?>("Command");

        public static ICommand? GetCommand(DataGrid o) => o.GetValue(CommandProperty);
        public static void SetCommand(DataGrid o, ICommand? value) => o.SetValue(CommandProperty, value);

        static DataGridLostKeyboardFocusCommandBehavior()
        {
            CommandProperty.Changed.AddClassHandler<DataGrid>(OnCommandChanged);
        }

        private static void OnCommandChanged(DataGrid dataGrid, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                dataGrid.LostFocus -= OnLostFocus;
            }

            if (e.NewValue != null)
            {
                dataGrid.LostFocus += OnLostFocus;
            }
        }

        // Откладываем до фонового приоритета, чтобы фокус успел перейти новому элементу.
        private static void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
            {
                return;
            }

            Dispatcher.UIThread.Post(
                () =>
                {
                    var focused = TopLevel.GetTopLevel(dataGrid)?.FocusManager?.GetFocusedElement() as Visual;
                    if (focused != null && dataGrid.IsVisualAncestorOf(focused))
                    {
                        return;
                    }

                    var command = GetCommand(dataGrid);
                    if (command?.CanExecute(null) == true)
                    {
                        command.Execute(null);
                    }
                },
                DispatcherPriority.Background);
        }
    }
}
