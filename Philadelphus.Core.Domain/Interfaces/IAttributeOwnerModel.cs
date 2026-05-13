using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
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
        public IReadOnlyList<ElementAttributeModel> Attributes { get; }

        /// <summary>
        /// Коллекция атрибутов собственных
        /// </summary>
        public IReadOnlyList<ElementAttributeModel> PersonalAttributes { get; }

        /// <summary>
        /// Коллекция атрибутов унаследованных
        /// </summary>
        public IReadOnlyList<ElementAttributeModel> ParentElementAttributes { get; }

        /// <summary>
        /// Добавить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool AddAttribute(ElementAttributeModel attribute);

        /// <summary>
        /// Удалить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool RemoveAttribute(ElementAttributeModel attribute);

        /// <summary>
        /// Очистить атрибуты
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool ClearAttributes();

        /// <summary>
        /// Получить видимые атрибуты родителей
        /// </summary>
        /// <param name="viewer">Текущий элемент</param>
        /// <returns>Результат выполнения операции.</returns>
        public IEnumerable<ElementAttributeModel> GetVisibleAttributesRecursive(IWorkingTreeMemberModel? viewer);

        /// <summary>
        /// Пометить как требующий пересчета списка атрибутов
        /// </summary>
        internal void MarkAsNeedRecalculateAttributesList();
    }
}
