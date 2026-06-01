namespace Philadelphus.Core.Domain.FormulaEngine.Diagnostics
{
    /// <summary>
    /// Тип диагностического события Formula Engine.
    /// </summary>
    public enum FormulaDiagnosticKind
    {
        /// <summary>
        /// Ошибка разбора текста формулы.
        /// </summary>
        ParseError,

        /// <summary>
        /// Ошибка вычисления корректно разобранной формулы.
        /// </summary>
        EvaluationError,

        /// <summary>
        /// Ошибка загрузки или регистрации формулы из расширения.
        /// </summary>
        PluginLoadError,

        /// <summary>
        /// Непредвиденное исключение во время вычисления.
        /// </summary>
        UnexpectedException
    }
}
