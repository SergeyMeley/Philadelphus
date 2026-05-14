using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs
{
    /// <summary>
    /// Задает контракт для работы с ILeaveParent.
    /// </summary>
    internal interface ILeaveParent
    {
        /// <summary>
        /// Создает объект листа рабочего дерева.
        /// </summary>
        /// <returns>Созданный объект.</returns>
        public TreeLeaveVM CreateTreeLeave();
    }
}
