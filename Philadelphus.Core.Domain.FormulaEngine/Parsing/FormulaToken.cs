using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Parsing
{
    /// <summary>
    /// Лексический токен формулы.
    /// </summary>
    public sealed class FormulaToken
    {
        /// <summary>
        /// Инициализирует лексический токен формулы.
        /// </summary>
        /// <param name="kind">Тип токена.</param>
        /// <param name="text">Исходный текст токена.</param>
        /// <param name="span">Диапазон токена в исходной формуле.</param>
        /// <param name="value">Разобранное значение токена, если оно есть.</param>
        public FormulaToken(
            FormulaTokenKind kind,
            string text,
            FormulaTextSpan span,
            object? value = null)
        {
            Kind = kind;
            Text = text;
            Span = span;
            Value = value;
        }

        /// <summary>
        /// Тип токена.
        /// </summary>
        public FormulaTokenKind Kind { get; }

        /// <summary>
        /// Исходный текст токена.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Диапазон токена в исходной формуле.
        /// </summary>
        public FormulaTextSpan Span { get; }

        /// <summary>
        /// Разобранное значение токена, если оно есть.
        /// </summary>
        public object? Value { get; }
    }
}
