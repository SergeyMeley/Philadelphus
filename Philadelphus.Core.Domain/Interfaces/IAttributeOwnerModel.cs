using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface IAttributeOwnerModel : IMainEntityModel, ILinkableByUuidModel
    {
        public bool HasAttributes { get; }
        public List<ElementAttributeModel> Attributes { get; }
        public List<ElementAttributeModel> PersonalAttributes { get; }
        public List<ElementAttributeModel> ParentElementAttributes { get; }
    }
}
