namespace Philadelphus.Core.Domain.FormulaEngine.Contracts
{
    /// <summary>
    /// Поставщик формул для регистрации в реестре.
    /// </summary>
    public interface IFormulaProvider
    {
        /// <summary>
        /// Возвращает определения формул, предоставляемых поставщиком.
        /// </summary>
        /// <returns>Коллекция определений формул.</returns>
        IEnumerable<FormulaDefinition> GetFormulas();
    }
}
