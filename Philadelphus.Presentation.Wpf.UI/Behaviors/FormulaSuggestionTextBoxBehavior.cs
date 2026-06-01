using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Attached behavior для навигации и подстановки предложений автодополнения формул в TextBox.
    /// </summary>
    public static class FormulaSuggestionTextBoxBehavior
    {
        /// <summary>
        /// Признак подключения поведения к TextBox.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(FormulaSuggestionTextBoxBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>
        /// ListBox с предложениями автодополнения, который нужно прокручивать при навигации.
        /// </summary>
        public static readonly DependencyProperty SuggestionsListBoxProperty =
            DependencyProperty.RegisterAttached(
                "SuggestionsListBox",
                typeof(ListBox),
                typeof(FormulaSuggestionTextBoxBehavior),
                new PropertyMetadata(null));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static ListBox? GetSuggestionsListBox(DependencyObject obj)
        {
            return (ListBox?)obj.GetValue(SuggestionsListBoxProperty);
        }

        public static void SetSuggestionsListBox(DependencyObject obj, ListBox? value)
        {
            obj.SetValue(SuggestionsListBoxProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not TextBox textBox)
            {
                return;
            }

            if (e.OldValue is true)
            {
                textBox.PreviewKeyDown -= OnPreviewKeyDown;
                textBox.SelectionChanged -= OnSelectionChanged;
            }

            if (e.NewValue is true)
            {
                textBox.PreviewKeyDown += OnPreviewKeyDown;
                textBox.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox
                || textBox.DataContext is not IFormulaEditorIntelliSenseVM viewModel)
            {
                return;
            }

            if (TryHandleFormulaNavigation(textBox, viewModel, e))
            {
                return;
            }

            if (viewModel.IsFormulaSuggestionsOpen == false)
            {
                return;
            }

            if (e.Key == Key.Down)
            {
                viewModel.MoveFormulaSuggestionSelection(1);
                ScrollSelectedSuggestionIntoView(textBox, viewModel);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up)
            {
                viewModel.MoveFormulaSuggestionSelection(-1);
                ScrollSelectedSuggestionIntoView(textBox, viewModel);
                e.Handled = true;
                return;
            }

            if (e.Key is Key.Tab or Key.Enter)
            {
                var caretIndex = viewModel.ApplySelectedFormulaSuggestion(textBox.SelectionStart);
                textBox.SelectionStart = caretIndex;
                textBox.SelectionLength = 0;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                viewModel.CloseFormulaSuggestions();
                e.Handled = true;
            }
        }

        private static void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox
                && textBox.DataContext is IFormulaEditorIntelliSenseVM viewModel)
            {
                viewModel.UpdateFormulaSuggestions(textBox.SelectionStart);
            }
        }

        private static void ScrollSelectedSuggestionIntoView(TextBox textBox, IFormulaEditorIntelliSenseVM viewModel)
        {
            GetSuggestionsListBox(textBox)?.ScrollIntoView(viewModel.SelectedFormulaSuggestion);
        }

        private static bool TryHandleFormulaNavigation(
            TextBox textBox,
            IFormulaEditorIntelliSenseVM viewModel,
            KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control
                && e.Key == Key.OemCloseBrackets
                && viewModel.TryGetMatchingParenthesisCaretIndex(textBox.SelectionStart, out var matchingCaretIndex))
            {
                textBox.SelectionStart = matchingCaretIndex;
                textBox.SelectionLength = 0;
                e.Handled = true;
                return true;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)
                && e.Key == Key.Space
                && viewModel.TryGetCurrentFormulaCallSelection(textBox.SelectionStart, out var selectionStart, out var selectionLength))
            {
                textBox.SelectionStart = selectionStart;
                textBox.SelectionLength = selectionLength;
                e.Handled = true;
                return true;
            }

            return false;
        }
    }
}
