using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    /// <summary>
    /// DTO для передачи данных рабочего дерева.
    /// </summary>
    public class WorkingTreeExportDTO
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Корень содержимого.
        /// </summary>
        public TreeRootExportDTO ContentRoot { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WorkingTreeExportDTO" />.
        /// </summary>
        /// <param name="name">Наименование.</param>
        /// <param name="root">Корень рабочего дерева.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
        public WorkingTreeExportDTO(string name, TreeRootExportDTO root) 
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(root);

            Name = name;
            ContentRoot = root;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WorkingTreeExportDTO" />.
        /// </summary>
        /// <param name="tree">Рабочее дерево.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public WorkingTreeExportDTO(WorkingTreeModel tree)
        {
            ArgumentNullException.ThrowIfNull(tree);
            ArgumentNullException.ThrowIfNull(tree.ContentRoot);

            Name = tree.Name;
            ContentRoot = new TreeRootExportDTO(tree.ContentRoot);
        }
    }
}
