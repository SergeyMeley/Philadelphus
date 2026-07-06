using Philadelphus.Core.Domain.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

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

        /// <summary>
        /// Все родители (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IParentModel> AllParentsRecursive { get; }

        /// <summary>
        /// Путь порядковых номеров от родителя к текущему наследнику.
        /// </summary>
        [Display(Name = "[№]", Description = "Путь порядковых номеров")]
        public string SequencePath => SequencePathHelper.GetSequencePath(this);

        /// <summary>
        /// Сменить родителя
        /// </summary>
        /// <param name="newParent">Новый родительский элемент.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool ChangeParent(IParentModel newParent);
    }
}
