using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    /// <summary>
    /// Задает контракт для работы с INodeParent.
    /// </summary>
    internal interface INodeParent
    {
        /// <summary>
        /// Создает объект узла рабочего дерева.
        /// </summary>
        /// <returns>Созданный объект.</returns>
        public TreeNodeVM CreateTreeNode();
    }
}
