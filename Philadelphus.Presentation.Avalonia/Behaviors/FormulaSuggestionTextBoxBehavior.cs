using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;

using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Навигация и подстановка предложений автодополнения формул в TextBox.
    /// Avalonia-аналог WPF-behavior: tunnel-обработка клавиш, обновление предложений по смене каретки.
    /// </summary>
    public sealed class FormulaSuggestionTextBoxBehavior
    {
        private FormulaSuggestionTextBoxBehavior()
        {
        }

        /// <summary>Признак подключения поведения к TextBox.</summary>
        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<FormulaSuggestionTextBoxBehavior, TextBox, bool>("IsEnabled");

        public static bool GetIsEnabled(TextBox o) => o.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(TextBox o, bool value) => o.SetValue(IsEnabledProperty, value);

        /// <summary>ListBox с предложениями автодополнения для прокрутки к выбранному.</summary>
        public static readonly AttachedProperty<ListBox?> SuggestionsListBoxProperty =
            AvaloniaProperty.RegisterAttached<FormulaSuggestionTextBoxBehavior, TextBox, ListBox?>("SuggestionsListBox");

        public static ListBox? GetSuggestionsListBox(TextBox o) => o.GetValue(SuggestionsListBoxProperty);
        public static void SetSuggestionsListBox(TextBox o, ListBox? value) => o.SetValue(SuggestionsListBoxProperty, value);

        static FormulaSuggestionTextBoxBehavior()
        {
            IsEnabledProperty.Changed.AddClassHandler<TextBox>(OnIsEnabledChanged);
        }

        private static void OnIsEnabledChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is true)
            {
                textBox.RemoveHandler(InputElement.KeyDownEvent, OnPreviewKeyDown);
                textBox.PropertyChanged -= OnTextBoxPropertyChanged;
            }

            if (e.NewValue is true)
            {
                // Tunnel: перехватываем стрелки/Tab/Enter до того, как их обработает TextBox.
                textBox.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
                textBox.PropertyChanged += OnTextBoxPropertyChanged;
            }
        }

        private static void OnPreviewKeyDown(object? sender, KeyEventArgs e)
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
                textBox.SelectionEnd = caretIndex;
                textBox.CaretIndex = caretIndex;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                viewModel.CloseFormulaSuggestions();
                e.Handled = true;
            }
        }

        private static void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is not TextBox textBox
                || textBox.DataContext is not IFormulaEditorIntelliSenseVM viewModel)
            {
                return;
            }

            if (e.Property == TextBox.CaretIndexProperty
                || e.Property == TextBox.SelectionStartProperty)
            {
                viewModel.UpdateFormulaSuggestions(textBox.SelectionStart);
            }
        }

        private static void ScrollSelectedSuggestionIntoView(TextBox textBox, IFormulaEditorIntelliSenseVM viewModel)
        {
            var listBox = GetSuggestionsListBox(textBox);
            if (listBox != null && viewModel.SelectedFormulaSuggestion != null)
            {
                listBox.ScrollIntoView(viewModel.SelectedFormulaSuggestion);
            }
        }

        private static bool TryHandleFormulaNavigation(
            TextBox textBox,
            IFormulaEditorIntelliSenseVM viewModel,
            KeyEventArgs e)
        {
            // Ctrl+] переносит каретку к парной скобке, как в редакторе формул.
            if (e.KeyModifiers == KeyModifiers.Control
                && e.Key == Key.OemCloseBrackets
                && viewModel.TryGetMatchingParenthesisCaretIndex(textBox.SelectionStart, out var matchingCaretIndex))
            {
                textBox.SelectionStart = matchingCaretIndex;
                textBox.SelectionEnd = matchingCaretIndex;
                textBox.CaretIndex = matchingCaretIndex;
                e.Handled = true;
                return true;
            }

            // Ctrl+Shift+Space выделяет текущий вызов функции.
            if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift)
                && e.Key == Key.Space
                && viewModel.TryGetCurrentFormulaCallSelection(textBox.SelectionStart, out var selectionStart, out var selectionLength))
            {
                textBox.SelectionStart = selectionStart;
                textBox.SelectionEnd = selectionStart + selectionLength;
                e.Handled = true;
                return true;
            }

            return false;
        }
    }
}
