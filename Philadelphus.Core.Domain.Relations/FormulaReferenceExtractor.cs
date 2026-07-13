using Philadelphus.Core.Domain.FormulaEngine.Expressions;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;

namespace Philadelphus.Core.Domain.Relations;

/// <summary>
/// Извлекает структурированные ссылки из синтаксического дерева формулы.
/// </summary>
public static class FormulaReferenceExtractor
{
    /// <summary>
    /// Возвращает уникальные UUID листов, явно указанные в формуле.
    /// </summary>
    /// <param name="formula">Текст анализируемой формулы.</param>
    /// <returns>Множество UUID листов, найденных в синтаксическом дереве формулы.</returns>
    public static IReadOnlySet<Guid> GetTreeLeaveUuids(string? formula)
    {
        var result = new HashSet<Guid>();
        var parsed = FormulaParser.Parse(formula);
        if (parsed.Expression != null) Visit(parsed.Expression, result);
        return result;
    }

    /// <summary>
    /// Рекурсивно обходит выражение и добавляет найденные ссылки.
    /// </summary>
    /// <param name="expression">Текущее выражение синтаксического дерева.</param>
    /// <param name="result">Коллекция найденных UUID листов.</param>
    private static void Visit(FormulaExpression expression, ISet<Guid> result)
    {
        switch (expression)
        {
            case TreeLeaveReferenceFormulaExpression reference: result.Add(reference.Uuid); break;
            case BinaryFormulaExpression binary: Visit(binary.Left, result); Visit(binary.Right, result); break;
            case ConditionalFormulaExpression conditional:
                Visit(conditional.Condition, result); Visit(conditional.WhenTrue, result); Visit(conditional.WhenFalse, result); break;
            case FunctionCallFormulaExpression function:
                foreach (var argument in function.Arguments) Visit(argument, result); break;
            case ObjectMethodCallFormulaExpression method:
                Visit(method.Target, result);
                foreach (var argument in method.Arguments) Visit(argument, result); break;
        }
    }
}
