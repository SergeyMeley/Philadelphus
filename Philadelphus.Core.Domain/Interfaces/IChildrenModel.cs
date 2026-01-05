namespace Philadelphus.Core.Domain.Interfaces
{
    public interface IChildrenModel : ILinkableByUuidModel
    {
        public IParentModel Parent { get; }
        //public Guid ParentUuid { get; }
    }
}
