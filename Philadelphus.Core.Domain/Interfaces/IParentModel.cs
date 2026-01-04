namespace Philadelphus.Core.Domain.Interfaces
{
    public interface IParentModel : ILinkableByUuidModel
    {
        List<IChildrenModel> Childs { get; }
    }
}
