namespace Philadelphus.Core.Domain.FormulaEngine.Errors
{
    /// <summary>
    /// Диапазон текста внутри исходной строки формулы.
    /// </summary>
    /// <param name="Start">Начальная позиция диапазона.</param>
    /// <param name="Length">Длина диапазона.</param>
    public readonly record struct FormulaTextSpan(int Start, int Length);
}
