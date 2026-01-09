namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Наследник
    /// </summary>
    public interface IChildrenModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Родитель
        /// </summary>
        public IParentModel Parent { get; }
    }
}
