using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Вызов функции формулы.
    /// </summary>
    /// <param name="Name">Имя вызываемой функции.</param>
    /// <param name="Arguments">Аргументы вызова функции.</param>
    /// <param name="Span">Диапазон вызова в исходной формуле.</param>
    public sealed record FunctionCallFormulaExpression(
        string Name,
        IReadOnlyList<FormulaExpression> Arguments,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
