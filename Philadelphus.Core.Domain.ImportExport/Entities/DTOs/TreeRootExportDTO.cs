using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ImportExport.Entities.DTOs
{
    /// <summary>
    /// DTO для передачи данных корня рабочего дерева.
    /// </summary>
    public class TreeRootExportDTO
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Выполняет операцию ChildNodes.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public List<TreeNodeExportDTO> ChildNodes { get; set; } = new();

        /// <summary>
        /// Выполняет операцию Attributes.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public List<AttributeExportDTO> Attributes { get; set; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeRootExportDTO" />.
        /// </summary>
        public TreeRootExportDTO()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeRootExportDTO" />.
        /// </summary>
        /// <param name="root">Корень рабочего дерева.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeRootExportDTO(TreeRootModel root)
        {
            ArgumentNullException.ThrowIfNull(root);

            Name = root.Name;
            Description = root.Description;
            ChildNodes = root.ChildNodes?.Select(n => new TreeNodeExportDTO(n)).ToList() ?? new();
            Attributes = root.Attributes?.Select(a => new AttributeExportDTO(a)).ToList() ?? new();
        }
    }
}
