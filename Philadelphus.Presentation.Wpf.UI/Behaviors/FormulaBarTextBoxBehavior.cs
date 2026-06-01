using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Передает во ViewModel состояние фокуса и позицию каретки строки формул.
    /// </summary>
    public static class FormulaBarTextBoxBehavior
    {
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.RegisterAttached(
                "IsEditing",
                typeof(bool),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static bool GetIsEditing(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEditingProperty);
        }

        public static void SetIsEditing(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEditingProperty, value);
        }

        public static readonly DependencyProperty CaretIndexProperty =
            DependencyProperty.RegisterAttached(
                "CaretIndex",
                typeof(int),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaretIndexChanged));

        public static int GetCaretIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(CaretIndexProperty);
        }

        public static void SetCaretIndex(DependencyObject obj, int value)
        {
            obj.SetValue(CaretIndexProperty, value);
        }

        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.RegisterAttached(
                "SelectionStart",
                typeof(int),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionStartChanged));

        public static int GetSelectionStart(DependencyObject obj)
        {
            return (int)obj.GetValue(SelectionStartProperty);
        }

        public static void SetSelectionStart(DependencyObject obj, int value)
        {
            obj.SetValue(SelectionStartProperty, value);
        }

        public static readonly DependencyProperty SelectionLengthProperty =
            DependencyProperty.RegisterAttached(
                "SelectionLength",
                typeof(int),
                typeof(FormulaBarTextBoxBehavior),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionLengthChanged));

        public static int GetSelectionLength(DependencyObject obj)
        {
            return (int)obj.GetValue(SelectionLengthProperty);
        }

        public static void SetSelectionLength(DependencyObject obj, int value)
        {
            obj.SetValue(SelectionLengthProperty, value);
        }

        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.RegisterAttached(
                "Track",
                typeof(bool),
                typeof(FormulaBarTextBoxBehavior),
                new PropertyMetadata(false, OnTrackChanged));

        public static bool GetTrack(DependencyObject obj)
        {
            return (bool)obj.GetValue(TrackProperty);
        }

        public static void SetTrack(DependencyObject obj, bool value)
        {
            obj.SetValue(TrackProperty, value);
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
                SelectFormulaOperandAtCaret(textBox);
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
            if (TryFindAttributeReferenceAtCaret(text, caretIndex, out var start, out var length)
                || TryFindOperandAtCaret(text, caretIndex, out start, out length))
            {
                textBox.SelectionStart = start;
                textBox.SelectionLength = length;
                UpdateSelection(textBox);
            }
        }

        private static bool TryFindAttributeReferenceAtCaret(
            string text,
            int caretIndex,
            out int referenceStart,
            out int referenceLength)
        {
            referenceStart = 0;
            referenceLength = 0;

            const string functionName = "АТРИБУТ";
            var searchIndex = 0;

            while (searchIndex < text.Length)
            {
                var nameIndex = text.IndexOf(functionName, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (nameIndex < 0)
                {
                    return false;
                }

                var openParenthesisIndex = SkipWhiteSpace(text, nameIndex + functionName.Length);
                if (openParenthesisIndex >= text.Length
                    || text[openParenthesisIndex] != '('
                    || TryFindClosingParenthesis(text, openParenthesisIndex, out var closeParenthesisIndex) == false)
                {
                    searchIndex = nameIndex + functionName.Length;
                    continue;
                }

                var endIndex = closeParenthesisIndex + 1;
                if (caretIndex >= nameIndex && caretIndex <= endIndex)
                {
                    referenceStart = nameIndex;
                    referenceLength = endIndex - nameIndex;
                    return true;
                }

                searchIndex = endIndex;
            }

            return false;
        }

        private static bool TryFindOperandAtCaret(
            string text,
            int caretIndex,
            out int operandStart,
            out int operandLength)
        {
            operandStart = 0;
            operandLength = 0;

            if (text.Length == 0)
            {
                return false;
            }

            var tokenStart = Math.Clamp(caretIndex, 0, text.Length);
            var tokenEnd = tokenStart;

            while (tokenStart > 0 && char.IsWhiteSpace(text[tokenStart - 1]))
            {
                tokenStart--;
            }

            if (tokenStart > 0 && text[tokenStart - 1] == '"')
            {
                return TryFindStringLiteralBeforeCaret(text, tokenStart, out operandStart, out operandLength);
            }

            while (tokenStart > 0 && IsOperandBoundary(text[tokenStart - 1]) == false)
            {
                tokenStart--;
            }

            while (tokenEnd < text.Length && char.IsWhiteSpace(text[tokenEnd]))
            {
                tokenEnd++;
            }

            while (tokenEnd < text.Length && IsOperandBoundary(text[tokenEnd]) == false)
            {
                tokenEnd++;
            }

            if (tokenEnd <= tokenStart)
            {
                return false;
            }

            var token = text[tokenStart..tokenEnd].Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            operandStart = tokenStart;
            operandLength = tokenEnd - tokenStart;
            return true;
        }

        private static bool TryFindStringLiteralBeforeCaret(
            string text,
            int caretIndex,
            out int operandStart,
            out int operandLength)
        {
            operandStart = 0;
            operandLength = 0;

            var start = caretIndex - 1;
            while (start > 0)
            {
                start--;
                if (text[start] != '"')
                {
                    continue;
                }

                var escapedQuotes = 0;
                for (var i = start + 1; i < caretIndex - 1 && text[i] == '"'; i++)
                {
                    escapedQuotes++;
                }

                if (escapedQuotes % 2 == 0)
                {
                    operandStart = start;
                    operandLength = caretIndex - start;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindClosingParenthesis(string text, int openParenthesisIndex, out int closeParenthesisIndex)
        {
            closeParenthesisIndex = -1;
            var depth = 0;
            var isInString = false;

            for (var i = openParenthesisIndex; i < text.Length; i++)
            {
                var current = text[i];
                if (current == '"')
                {
                    if (isInString && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    isInString = !isInString;
                    continue;
                }

                if (isInString)
                {
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                    continue;
                }

                if (current != ')')
                {
                    continue;
                }

                depth--;
                if (depth == 0)
                {
                    closeParenthesisIndex = i;
                    return true;
                }
            }

            return false;
        }

        private static int SkipWhiteSpace(string text, int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            return index;
        }

        private static bool IsOperandBoundary(char value)
        {
            return char.IsWhiteSpace(value)
                || value is ';' or ',' or '+' or '-' or '*' or '/' or '(' or ')' or '>' or '<' or '=' or '!';
        }
    }
}
