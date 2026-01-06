namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Идентифицируемый
    /// </summary>
    public interface ILinkableByUuidModel
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid { get; }
    }
}
