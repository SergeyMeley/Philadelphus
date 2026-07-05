using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    /// <summary>
    /// Задает контракт для работы с ILeaveParent.
    /// </summary>
    public interface ILeaveParent
    {
        /// <summary>
        /// Дочерние листья.
        /// </summary>
        public ObservableCollection<TreeLeaveVM> ChildLeaves { get; }

        /// <summary>
        /// Создает объект листа рабочего дерева.
        /// </summary>
        /// <returns>Созданный объект.</returns>
        public TreeLeaveVM CreateTreeLeave();
    }
}
