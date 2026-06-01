namespace Philadelphus.Core.Domain.FormulaEngine.Errors
{
    /// <summary>
    /// Подробная ошибка разбора или вычисления формулы.
    /// </summary>
    public sealed class FormulaError
    {
        /// <summary>
        /// Машинно-читаемый код ошибки.
        /// </summary>
        public required FormulaErrorCode Code { get; init; }

        /// <summary>
        /// Подробное сообщение для уведомления или журнала.
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        /// Диапазон исходной формулы, к которому относится ошибка.
        /// </summary>
        public FormulaTextSpan? Span { get; init; }

        /// <summary>
        /// Имя функции или оператора, при обработке которого возникла ошибка.
        /// </summary>
        public string? FunctionOrOperator { get; init; }

        /// <summary>
        /// Исходное исключение, если ошибка была получена из исключительной ситуации.
        /// </summary>
        public Exception? Exception { get; init; }
    }
}
