using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Владелец атрибутов
    /// </summary>
    public interface IAttributeOwnerModel : IOwnerModel, IMainEntityModel, ILinkableByUuidModel
    {
        /// <summary>
        /// Имеет атрибуты
        /// </summary>
        public bool HasAttributes { get; }

        /// <summary>
        /// Коллекция атрибутов (собственных и унаследованных)
        /// </summary>
        public List<ElementAttributeModel> Attributes { get; }

        /// <summary>
        /// Коллекция атрибутов собственных
        /// </summary>
        public List<ElementAttributeModel> PersonalAttributes { get; }

        /// <summary>
        /// Коллекция атрибутов унаследованных
        /// </summary>
        public List<ElementAttributeModel> ParentElementAttributes { get; }
    }
}
