using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    /// <summary>
    /// Задает контракт для работы с INodeParent.
    /// </summary>
    public interface INodeParent
    {
        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        public ObservableCollection<TreeNodeVM> ChildNodes { get; }

        /// <summary>
        /// Создает объект узла рабочего дерева.
        /// </summary>
        /// <returns>Созданный объект.</returns>
        public TreeNodeVM CreateTreeNode();
    }
}
