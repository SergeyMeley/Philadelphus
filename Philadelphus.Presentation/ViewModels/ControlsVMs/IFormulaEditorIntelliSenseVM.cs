using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Общий контракт редактора формул для подсказок, подсветки и навигации из attached behavior.
    /// </summary>
    public interface IFormulaEditorIntelliSenseVM
    {
        /// <summary>
        /// Текущие предложения автодополнения для позиции курсора.
        /// </summary>
        ObservableCollection<FormulaSuggestionVM> FormulaSuggestions { get; }

        /// <summary>
        /// Сегменты цветовой подсветки текущего текста формулы.
        /// </summary>
        ObservableCollection<FormulaHighlightSegmentVM> FormulaHighlightSegments { get; }

        /// <summary>
        /// Признак отображения выпадающего списка автодополнения.
        /// </summary>
        bool IsFormulaSuggestionsOpen { get; set; }

        /// <summary>
        /// Выбранное предложение автодополнения.
        /// </summary>
        FormulaSuggestionVM? SelectedFormulaSuggestion { get; set; }

        /// <summary>
        /// Признак отображения подсказки сигнатуры активной функции.
        /// </summary>
        bool IsFormulaSignatureHelpOpen { get; }

        /// <summary>
        /// Текст сигнатуры активной функции.
        /// </summary>
        string FormulaSignatureText { get; }

        /// <summary>
        /// Текст описания активного аргумента функции.
        /// </summary>
        string FormulaActiveArgumentText { get; }

        /// <summary>
        /// Признак отображения цветовой подсветки формулы.
        /// </summary>
        bool IsFormulaHighlightOpen { get; }

        /// <summary>
        /// Обновляет подсказки, сигнатуру и подсветку по позиции курсора.
        /// </summary>
        /// <param name="caretIndex">Позиция курсора в тексте формулы.</param>
        void UpdateFormulaSuggestions(int caretIndex);

        /// <summary>
        /// Смещает выбранную подсказку автодополнения.
        /// </summary>
        /// <param name="offset">Смещение выбора: положительное вниз, отрицательное вверх.</param>
        void MoveFormulaSuggestionSelection(int offset);

        /// <summary>
        /// Закрывает список подсказок автодополнения.
        /// </summary>
        void CloseFormulaSuggestions();

        /// <summary>
        /// Применяет выбранную подсказку автодополнения.
        /// </summary>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <returns>Новая позиция курсора после вставки.</returns>
        int ApplySelectedFormulaSuggestion(int caretIndex);

        /// <summary>
        /// Ищет позицию парной скобки для текущей позиции курсора.
        /// </summary>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <param name="matchingCaretIndex">Позиция парной скобки, если она найдена.</param>
        /// <returns>true, если парная скобка найдена; иначе false.</returns>
        bool TryGetMatchingParenthesisCaretIndex(int caretIndex, out int matchingCaretIndex);

        /// <summary>
        /// Ищет диапазон текущего вызова функции для выделения в редакторе.
        /// </summary>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <param name="selectionStart">Начало диапазона текущего вызова функции.</param>
        /// <param name="selectionLength">Длина диапазона текущего вызова функции.</param>
        /// <returns>true, если диапазон найден; иначе false.</returns>
        bool TryGetCurrentFormulaCallSelection(int caretIndex, out int selectionStart, out int selectionLength);
    }
}
