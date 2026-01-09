namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Содержимое сущности
    /// </summary>
    internal interface ITreeElementContentModel
    {
        /// <summary>
        /// Владелец содержимого
        /// </summary>
        IAttributeOwnerModel Owner { get; }
    }
}
