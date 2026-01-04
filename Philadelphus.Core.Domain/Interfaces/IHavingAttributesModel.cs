using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface IHavingAttributesModel
    {
        public Guid Uuid { get; }
        public IEnumerable<ElementAttributeModel> PersonalAttributes { get; }
        public IEnumerable<ElementAttributeModel> ParentElementAttributes { get; }
        public bool HasContent { get; set; }
    }
}
