using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Бинарное выражение формулы.
    /// </summary>
    /// <param name="Left">Левый операнд выражения.</param>
    /// <param name="Operator">Текстовый код оператора.</param>
    /// <param name="Right">Правый операнд выражения.</param>
    /// <param name="Span">Диапазон выражения в исходной формуле.</param>
    public sealed record BinaryFormulaExpression(
        FormulaExpression Left,
        string Operator,
        FormulaExpression Right,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
