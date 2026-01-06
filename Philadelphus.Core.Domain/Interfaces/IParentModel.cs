namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Родитель
    /// </summary>
    public interface IParentModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Наследник
        /// </summary>
        List<IChildrenModel> Childs { get; }
    }
}
