using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Родитель
    /// </summary>
    public interface IParentModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Наследники
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> Childs { get; }

        /// <summary>
        /// Все наследники (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> AllChildsRecursive { get; }

        /// <summary>
        /// Добавить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool AddChild(IChildrenModel child);

        /// <summary>
        /// Удалить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool RemoveChild(IChildrenModel child);

        /// <summary>
        /// Очистить список наследников
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool ClearChilds();
    }
}
