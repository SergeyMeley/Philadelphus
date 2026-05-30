namespace Philadelphus.Core.Domain.FormulaEngine.Registry
{
    /// <summary>
    /// Исключение регистрации формулы с некорректными или неоднозначными metadata.
    /// </summary>
    public sealed class FormulaRegistrationException : Exception
    {
        public FormulaRegistrationException(string message)
            : base(message)
        {
        }
    }
}
