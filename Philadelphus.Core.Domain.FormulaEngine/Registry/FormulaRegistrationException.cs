namespace Philadelphus.Core.Domain.FormulaEngine.Registry
{
    /// <summary>
    /// Исключение регистрации формулы с некорректными или неоднозначными metadata.
    /// </summary>
    public sealed class FormulaRegistrationException : Exception
    {
        /// <summary>
        /// Инициализирует исключение регистрации формулы.
        /// </summary>
        /// <param name="message">Описание ошибки регистрации.</param>
        public FormulaRegistrationException(string message)
            : base(message)
        {
        }
    }
}
