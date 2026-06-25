using Philadelphus.Presentation.Behaviors.Logic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Передает во ViewModel состояние фокуса и позицию каретки строки формул.
    /// </summary>
    public static class FormulaBarTextBoxBehavior
    {
        /// <summary>
        /// Признак активного редактирования строки формул.
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.RegisterAttached(
                "IsEditing",
                typeof(bool),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Возвращает признак активного редактирования строки формул.
        /// </summary>
        public static bool GetIsEditing(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEditingProperty);
        }

        /// <summary>
        /// Устанавливает признак активного редактирования строки формул.
        /// </summary>
        public static void SetIsEditing(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEditingProperty, value);
        }

        /// <summary>
        /// Позиция каретки в строке формул.
        /// </summary>
        public static readonly DependencyProperty CaretIndexProperty =
            DependencyProperty.RegisterAttached(
                "CaretIndex",
                typeof(int),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaretIndexChanged));

        /// <summary>
        /// Возвращает позицию каретки в строке формул.
        /// </summary>
        public static int GetCaretIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(CaretIndexProperty);
        }

        /// <summary>
        /// Устанавливает позицию каретки в строке формул.
        /// </summary>
        public static void SetCaretIndex(DependencyObject obj, int value)
        {
            obj.SetValue(CaretIndexProperty, value);
        }

        /// <summary>
        /// Начало выделения в строке формул.
        /// </summary>
        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.RegisterAttached(
                "SelectionStart",
                typeof(int),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionStartChanged));

        /// <summary>
        /// Возвращает начало выделения в строке формул.
        /// </summary>
        public static int GetSelectionStart(DependencyObject obj)
        {
            return (int)obj.GetValue(SelectionStartProperty);
        }

        /// <summary>
        /// Устанавливает начало выделения в строке формул.
        /// </summary>
        public static void SetSelectionStart(DependencyObject obj, int value)
        {
            obj.SetValue(SelectionStartProperty, value);
        }

        /// <summary>
        /// Длина выделения в строке формул.
        /// </summary>
        public static readonly DependencyProperty SelectionLengthProperty =
            DependencyProperty.RegisterAttached(
                "SelectionLength",
                typeof(int),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionLengthChanged));

        /// <summary>
        /// Возвращает длину выделения в строке формул.
        /// </summary>
        public static int GetSelectionLength(DependencyObject obj)
        {
            return (int)obj.GetValue(SelectionLengthProperty);
        }

        /// <summary>
        /// Устанавливает длину выделения в строке формул.
        /// </summary>
        public static void SetSelectionLength(DependencyObject obj, int value)
        {
            obj.SetValue(SelectionLengthProperty, value);
        }

        /// <summary>
        /// Признак подключения behavior к TextBox строки формул.
        /// </summary>
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.RegisterAttached(
                "Track",
                typeof(bool),
                typeof(FormulaBarTextBoxBehavior),
                new PropertyMetadata(false, OnTrackChanged));

        /// <summary>
        /// Идентификатор запроса возврата фокуса в строку формул.
        /// </summary>
        public static readonly DependencyProperty FocusRequestIdProperty =
            DependencyProperty.RegisterAttached(
                "FocusRequestId",
                typeof(int),
                typeof(FormulaBarTextBoxBehavior),
                new PropertyMetadata(0, OnFocusRequestIdChanged));

        /// <summary>
        /// Возвращает признак подключения behavior к TextBox строки формул.
        /// </summary>
        public static bool GetTrack(DependencyObject obj)
        {
            return (bool)obj.GetValue(TrackProperty);
        }

        /// <summary>
        /// Устанавливает признак подключения behavior к TextBox строки формул.
        /// </summary>
        public static void SetTrack(DependencyObject obj, bool value)
        {
            obj.SetValue(TrackProperty, value);
        }

        /// <summary>
        /// Возвращает идентификатор запроса возврата фокуса.
        /// </summary>
        public static int GetFocusRequestId(DependencyObject obj)
        {
            return (int)obj.GetValue(FocusRequestIdProperty);
        }

        /// <summary>
        /// Устанавливает идентификатор запроса возврата фокуса.
        /// </summary>
        public static void SetFocusRequestId(DependencyObject obj, int value)
        {
            obj.SetValue(FocusRequestIdProperty, value);
        }

        private static void OnTrackChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not TextBox textBox)
            {
                return;
            }

            if ((bool)e.OldValue)
            {
                textBox.GotKeyboardFocus -= OnGotKeyboardFocus;
                textBox.LostKeyboardFocus -= OnLostKeyboardFocus;
                textBox.SelectionChanged -= OnSelectionChanged;
                textBox.PreviewKeyUp -= OnPreviewKeyUp;
                textBox.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            }

            if ((bool)e.NewValue)
            {
                textBox.GotKeyboardFocus += OnGotKeyboardFocus;
                textBox.LostKeyboardFocus += OnLostKeyboardFocus;
                textBox.SelectionChanged += OnSelectionChanged;
                textBox.PreviewKeyUp += OnPreviewKeyUp;
                textBox.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            }
        }

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SetIsEditing(textBox, true);
                UpdateSelection(textBox);
            }
        }

        private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateSelection(textBox);
            }
        }

        private static void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateSelection(textBox);
            }
        }

        private static void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateSelection(textBox);
            }
        }

        private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateSelection(textBox);
                if (e.ClickCount >= 2)
                {
                    SelectFormulaOperandAtCaret(textBox);
                }
            }
        }

        private static void OnCaretIndexChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not TextBox textBox
                || e.NewValue is not int caretIndex
                || textBox.IsKeyboardFocusWithin
                || textBox.CaretIndex == caretIndex)
            {
                return;
            }

            textBox.CaretIndex = Math.Clamp(caretIndex, 0, textBox.Text?.Length ?? 0);
        }

        private static void OnSelectionStartChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not TextBox textBox
                || e.NewValue is not int selectionStart
                || textBox.IsKeyboardFocusWithin
                || textBox.SelectionStart == selectionStart)
            {
                return;
            }

            textBox.SelectionStart = Math.Clamp(selectionStart, 0, textBox.Text?.Length ?? 0);
        }

        private static void OnSelectionLengthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not TextBox textBox
                || e.NewValue is not int selectionLength
                || textBox.IsKeyboardFocusWithin
                || textBox.SelectionLength == selectionLength)
            {
                return;
            }

            var maxLength = Math.Max(0, (textBox.Text?.Length ?? 0) - textBox.SelectionStart);
            textBox.SelectionLength = Math.Clamp(selectionLength, 0, maxLength);
        }

        private static void OnFocusRequestIdChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not TextBox textBox
                || Equals(e.OldValue, e.NewValue))
            {
                return;
            }

            textBox.Dispatcher.BeginInvoke(
                () =>
                {
                    if (textBox.IsEnabled == false)
                    {
                        return;
                    }

                    textBox.Focus();
                    Keyboard.Focus(textBox);
                    textBox.SelectionStart = Math.Clamp(GetSelectionStart(textBox), 0, textBox.Text?.Length ?? 0);
                    textBox.SelectionLength = Math.Clamp(
                        GetSelectionLength(textBox),
                        0,
                        Math.Max(0, (textBox.Text?.Length ?? 0) - textBox.SelectionStart));
                    textBox.CaretIndex = Math.Clamp(GetCaretIndex(textBox), 0, textBox.Text?.Length ?? 0);
                },
                DispatcherPriority.Input);
        }

        private static void UpdateSelection(TextBox textBox)
        {
            SetCaretIndex(textBox, textBox.CaretIndex);
            SetSelectionStart(textBox, textBox.SelectionStart);
            SetSelectionLength(textBox, textBox.SelectionLength);
        }

        private static void SelectFormulaOperandAtCaret(TextBox textBox)
        {
            var text = textBox.Text ?? string.Empty;
            if (textBox.SelectionLength > 0
                || text.TrimStart().StartsWith("=", StringComparison.Ordinal) == false)
            {
                return;
            }

            var caretIndex = Math.Clamp(textBox.CaretIndex, 0, text.Length);
            if (FormulaOperandFinder.TryFindAttributeReferenceAtCaret(text, caretIndex, out var start, out var length)
                || FormulaOperandFinder.TryFindOperandAtCaret(text, caretIndex, out start, out length))
            {
                textBox.SelectionStart = start;
                textBox.SelectionLength = length;
                UpdateSelection(textBox);
            }
        }

    }
}
