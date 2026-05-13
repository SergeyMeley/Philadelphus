using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    /// <summary>
    /// DTO для передачи данных листа рабочего дерева.
    /// </summary>
    public class TreeLeaveExportDTO
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
        /// Наименование владеющего узла.
        /// </summary>
        public string OwningNodeName { get; }

        /// <summary>
        /// Выполняет операцию Attributes.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public List<AttributeExportDTO> Attributes { get; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeaveExportDTO" />.
        /// </summary>
        /// <param name="leave">Лист рабочего дерева.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public TreeLeaveExportDTO(TreeLeaveModel leave)
        {
            ArgumentNullException.ThrowIfNull(leave);

            Name = leave.Name;
            Description = leave.Description;
            OwningNodeName = leave.ParentNode?.Name ?? "Неизвестный";
            Attributes = leave.Attributes?.Select(a => new AttributeExportDTO(a)).ToList() ?? new();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeaveExportDTO" />.
        /// </summary>
        /// <param name="name">Наименование.</param>
        /// <param name="description">Описание.</param>
        /// <param name="owningNodeName">Наименование владеющего узла.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
        public TreeLeaveExportDTO(string name, string description, string owningNodeName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentException.ThrowIfNullOrWhiteSpace(owningNodeName);

            Name = name;
            Description = description;
            OwningNodeName = owningNodeName;
        }
    }
}
