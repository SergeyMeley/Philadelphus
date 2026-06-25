using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using System.Linq;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Предложение автодополнения редактора формул.
    /// </summary>
    public sealed class FormulaSuggestionVM
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FormulaSuggestionVM" />.
        /// </summary>
        /// <param name="displayName">Отображаемое имя предложения.</param>
        /// <param name="insertName">Имя, подставляемое в формулу.</param>
        /// <param name="formula">Описание формулы.</param>
        /// <param name="useTemplate">Признак подстановки шаблона вызова функции.</param>
        public FormulaSuggestionVM(
            string displayName,
            string insertName,
            FormulaDefinition formula,
            bool useTemplate)
        {
            DisplayName = displayName;
            InsertName = insertName;
            Formula = formula;
            UseTemplate = useTemplate;
        }

        /// <summary>
        /// Отображаемое имя предложения.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Имя, подставляемое в формулу.
        /// </summary>
        public string InsertName { get; }

        /// <summary>
        /// Описание формулы.
        /// </summary>
        public FormulaDefinition Formula { get; }

        /// <summary>
        /// Признак подстановки шаблона вызова функции.
        /// </summary>
        public bool UseTemplate { get; }

        /// <summary>
        /// Категория формулы.
        /// </summary>
        public string Category => Formula.Category ?? "Формулы";

        /// <summary>
        /// Описание формулы.
        /// </summary>
        public string Description => Formula.Description ?? string.Empty;

        /// <summary>
        /// Сигнатура для отображения в списке предложений.
        /// </summary>
        public string Signature
        {
            get
            {
                var arguments = Formula.Arguments.Count == 0
                    ? string.Empty
                    : string.Join("; ", Formula.Arguments.Select(argument => argument.Name));

                return $"{Formula.Name}({arguments})";
            }
        }

        /// <summary>
        /// Создает текст подстановки и новую позицию курсора.
        /// </summary>
        /// <returns>Результат подстановки.</returns>
        public FormulaCompletionResult CreateCompletion()
        {
            if (UseTemplate == false)
            {
                return new FormulaCompletionResult(InsertName, InsertName.Length);
            }

            if (Formula.Arguments.Count <= 1)
            {
                return new FormulaCompletionResult($"{InsertName}()", InsertName.Length + 1);
            }

            var separators = string.Join(string.Empty, Enumerable.Repeat("; ", Formula.Arguments.Count - 1));
            return new FormulaCompletionResult($"{InsertName}({separators})", InsertName.Length + 1);
        }
    }

    /// <summary>
    /// Результат подстановки предложения автодополнения.
    /// </summary>
    /// <param name="Text">Текст подстановки.</param>
    /// <param name="CaretOffset">Позиция курсора относительно начала подстановки.</param>
    public sealed record FormulaCompletionResult(string Text, int CaretOffset);

    /// <summary>
    /// Сегмент визуальной подсветки формулы.
    /// </summary>
    /// <param name="Text">Текст сегмента.</param>
    /// <param name="Kind">Тип визуальной подсветки.</param>
    /// <param name="IsMatchingParenthesis">Признак парной скобки рядом с курсором.</param>
    public sealed record FormulaHighlightSegmentVM(
        string Text,
        FormulaHighlightKind Kind,
        bool IsMatchingParenthesis);

    /// <summary>
    /// Тип визуальной подсветки сегмента формулы.
    /// </summary>
    public enum FormulaHighlightKind
    {
        Default,
        Function,
        Identifier,
        Number,
        String,
        TreeLeaveReference,
        Parenthesis,
        Punctuation,
        Operator,
        Error
    }

    /// <summary>
    /// Текущий открытый вызов функции в тексте формулы.
    /// </summary>
    /// <param name="Name">Имя функции.</param>
    /// <param name="ArgumentIndex">Индекс активного аргумента.</param>
    public sealed record ActiveFormulaCall(string Name, int ArgumentIndex);
}