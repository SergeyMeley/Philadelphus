using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Объектный вызов функции от результата другого выражения.
    /// </summary>
    /// <param name="Target">Выражение, над результатом которого вызывается метод.</param>
    /// <param name="MethodName">Имя объектного метода.</param>
    /// <param name="Arguments">Аргументы объектного метода.</param>
    /// <param name="Span">Диапазон объектного вызова в исходной формуле.</param>
    public sealed record ObjectMethodCallFormulaExpression(
        FormulaExpression Target,
        string MethodName,
        IReadOnlyList<FormulaExpression> Arguments,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
