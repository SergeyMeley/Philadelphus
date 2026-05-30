namespace Philadelphus.Core.Domain.FormulaEngine.Errors
{
    /// <summary>
    /// Диапазон текста внутри исходной строки формулы.
    /// </summary>
    public readonly record struct FormulaTextSpan(int Start, int Length);
}
