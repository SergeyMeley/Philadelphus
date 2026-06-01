using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public interface IFormulaEditorIntelliSenseVM
    {
        ObservableCollection<FormulaSuggestionVM> FormulaSuggestions { get; }

        ObservableCollection<FormulaHighlightSegmentVM> FormulaHighlightSegments { get; }

        bool IsFormulaSuggestionsOpen { get; set; }

        FormulaSuggestionVM? SelectedFormulaSuggestion { get; set; }

        bool IsFormulaSignatureHelpOpen { get; }

        string FormulaSignatureText { get; }

        string FormulaActiveArgumentText { get; }

        bool IsFormulaHighlightOpen { get; }

        void UpdateFormulaSuggestions(int caretIndex);

        void MoveFormulaSuggestionSelection(int offset);

        void CloseFormulaSuggestions();

        int ApplySelectedFormulaSuggestion(int caretIndex);

        bool TryGetMatchingParenthesisCaretIndex(int caretIndex, out int matchingCaretIndex);

        bool TryGetCurrentFormulaCallSelection(int caretIndex, out int selectionStart, out int selectionLength);
    }
}
