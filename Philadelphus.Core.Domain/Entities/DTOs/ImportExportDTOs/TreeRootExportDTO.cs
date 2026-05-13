using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    /// <summary>
    /// DTO для передачи данных корня рабочего дерева.
    /// </summary>
    public class TreeRootExportDTO
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Выполняет операцию ChildNodes.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public List<TreeNodeExportDTO> ChildNodes { get; } = new();

        /// <summary>
        /// Выполняет операцию Attributes.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public List<AttributeExportDTO> Attributes { get; set; } = new();

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

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeRootExportDTO" />.
        /// </summary>
        /// <param name="name">Наименование.</param>
        /// <param name="description">Описание.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
        public TreeRootExportDTO(string name, string description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(description);

            Name = name;
            Description = description;
        }
    }
}
