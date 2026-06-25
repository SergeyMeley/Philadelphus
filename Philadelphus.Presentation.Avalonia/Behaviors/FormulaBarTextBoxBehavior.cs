using System;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;

using Philadelphus.Presentation.Behaviors.Logic;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Передает во ViewModel состояние фокуса и позицию каретки/выделения строки формул.
    /// Avalonia-аналог одноименного WPF-behavior (в Avalonia TextBox использует SelectionStart/SelectionEnd,
    /// нет SelectionChanged — отслеживаем через PropertyChanged).
    /// </summary>
    public class FormulaBarTextBoxBehavior
    {
        private FormulaBarTextBoxBehavior()
        {
        }

        /// <summary>Признак активного редактирования строки формул.</summary>
        public static readonly AttachedProperty<bool> IsEditingProperty =
            AvaloniaProperty.RegisterAttached<FormulaBarTextBoxBehavior, TextBox, bool>("IsEditing");

        public static bool GetIsEditing(TextBox o) => o.GetValue(IsEditingProperty);
        public static void SetIsEditing(TextBox o, bool value) => o.SetValue(IsEditingProperty, value);

        /// <summary>Позиция каретки в строке формул.</summary>
        public static readonly AttachedProperty<int> CaretIndexProperty =
            AvaloniaProperty.RegisterAttached<FormulaBarTextBoxBehavior, TextBox, int>("CaretIndex");

        public static int GetCaretIndex(TextBox o) => o.GetValue(CaretIndexProperty);
        public static void SetCaretIndex(TextBox o, int value) => o.SetValue(CaretIndexProperty, value);

        /// <summary>Начало выделения в строке формул.</summary>
        public static readonly AttachedProperty<int> SelectionStartProperty =
            AvaloniaProperty.RegisterAttached<FormulaBarTextBoxBehavior, TextBox, int>("SelectionStart");

        public static int GetSelectionStart(TextBox o) => o.GetValue(SelectionStartProperty);
        public static void SetSelectionStart(TextBox o, int value) => o.SetValue(SelectionStartProperty, value);

        /// <summary>Длина выделения в строке формул.</summary>
        public static readonly AttachedProperty<int> SelectionLengthProperty =
            AvaloniaProperty.RegisterAttached<FormulaBarTextBoxBehavior, TextBox, int>("SelectionLength");

        public static int GetSelectionLength(TextBox o) => o.GetValue(SelectionLengthProperty);
        public static void SetSelectionLength(TextBox o, int value) => o.SetValue(SelectionLengthProperty, value);

        /// <summary>Признак подключения behavior к TextBox строки формул.</summary>
        public static readonly AttachedProperty<bool> TrackProperty =
            AvaloniaProperty.RegisterAttached<FormulaBarTextBoxBehavior, TextBox, bool>("Track");

        public static bool GetTrack(TextBox o) => o.GetValue(TrackProperty);
        public static void SetTrack(TextBox o, bool value) => o.SetValue(TrackProperty, value);

        /// <summary>Идентификатор запроса возврата фокуса в строку формул.</summary>
        public static readonly AttachedProperty<int> FocusRequestIdProperty =
            AvaloniaProperty.RegisterAttached<FormulaBarTextBoxBehavior, TextBox, int>("FocusRequestId");

        public static int GetFocusRequestId(TextBox o) => o.GetValue(FocusRequestIdProperty);
        public static void SetFocusRequestId(TextBox o, int value) => o.SetValue(FocusRequestIdProperty, value);

        static FormulaBarTextBoxBehavior()
        {
            TrackProperty.Changed.AddClassHandler<TextBox>(OnTrackChanged);
            CaretIndexProperty.Changed.AddClassHandler<TextBox>((tb, _) => ApplyFromViewModel(tb));
            SelectionStartProperty.Changed.AddClassHandler<TextBox>((tb, _) => ApplyFromViewModel(tb));
            SelectionLengthProperty.Changed.AddClassHandler<TextBox>((tb, _) => ApplyFromViewModel(tb));
            FocusRequestIdProperty.Changed.AddClassHandler<TextBox>(OnFocusRequestIdChanged);
        }

        private static void OnTrackChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is true)
            {
                textBox.GotFocus -= OnGotFocus;
                textBox.LostFocus -= OnLostFocus;
                textBox.PropertyChanged -= OnTextBoxPropertyChanged;
                textBox.DoubleTapped -= OnDoubleTapped;
            }

            if (e.NewValue is true)
            {
                textBox.GotFocus += OnGotFocus;
                textBox.LostFocus += OnLostFocus;
                textBox.PropertyChanged += OnTextBoxPropertyChanged;
                textBox.DoubleTapped += OnDoubleTapped;
            }
        }

        private static void OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SetIsEditing(textBox, true);
                PushSelectionToViewModel(textBox);
            }
        }

        private static void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                PushSelectionToViewModel(textBox);
            }
        }

        private static void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (e.Property == TextBox.CaretIndexProperty
                || e.Property == TextBox.SelectionStartProperty
                || e.Property == TextBox.SelectionEndProperty
                || e.Property == TextBox.TextProperty)
            {
                PushSelectionToViewModel(textBox);
            }
        }

        private static void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SelectFormulaOperandAtCaret(textBox);
            }
        }

        private static void OnFocusRequestIdChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
        {
            if (Equals(e.OldValue, e.NewValue))
            {
                return;
            }

            Dispatcher.UIThread.Post(
                () =>
                {
                    if (textBox.IsEnabled == false)
                    {
                        return;
                    }

                    textBox.Focus();
                    ApplyFromViewModel(textBox);
                },
                DispatcherPriority.Input);
        }

        /// <summary>Текстбокс → ViewModel: переносит каретку и выделение в attached-свойства.</summary>
        private static void PushSelectionToViewModel(TextBox textBox)
        {
            var start = Math.Min(textBox.SelectionStart, textBox.SelectionEnd);
            var length = Math.Abs(textBox.SelectionEnd - textBox.SelectionStart);

            SetCaretIndex(textBox, textBox.CaretIndex);
            SetSelectionStart(textBox, start);
            SetSelectionLength(textBox, length);
        }

        /// <summary>ViewModel → текстбокс: применяет каретку/выделение, только когда поле не в фокусе.</summary>
        private static void ApplyFromViewModel(TextBox textBox)
        {
            if (textBox.IsFocused)
            {
                return;
            }

            var textLength = textBox.Text?.Length ?? 0;
            var start = Math.Clamp(GetSelectionStart(textBox), 0, textLength);
            var length = Math.Clamp(GetSelectionLength(textBox), 0, Math.Max(0, textLength - start));

            textBox.SelectionStart = start;
            textBox.SelectionEnd = start + length;
            textBox.CaretIndex = Math.Clamp(GetCaretIndex(textBox), 0, textLength);
        }

        private static void SelectFormulaOperandAtCaret(TextBox textBox)
        {
            var text = textBox.Text ?? string.Empty;
            var selectionLength = Math.Abs(textBox.SelectionEnd - textBox.SelectionStart);
            if (selectionLength > 0
                || text.TrimStart().StartsWith("=", StringComparison.Ordinal) == false)
            {
                return;
            }

            var caretIndex = Math.Clamp(textBox.CaretIndex, 0, text.Length);
            if (FormulaOperandFinder.TryFindAttributeReferenceAtCaret(text, caretIndex, out var start, out var length)
                || FormulaOperandFinder.TryFindOperandAtCaret(text, caretIndex, out start, out length))
            {
                textBox.SelectionStart = start;
                textBox.SelectionEnd = start + length;
                PushSelectionToViewModel(textBox);
            }
        }
    }
}
