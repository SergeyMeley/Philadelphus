using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ImportExport.Entities.DTOs
{
    /// <summary>
    /// DTO для передачи данных листа рабочего дерева.
    /// </summary>
    public class TreeLeaveExportDTO
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
        /// Строковое значение листа.
        /// </summary>
        public string StringValue { get; set; } = TreeLeaveModel.EmptyStringValue;

        /// <summary>
        /// Наименование владеющего узла.
        /// </summary>
        public string OwningNodeName { get; set; } = string.Empty;

        /// <summary>
        /// Атрибуты листа, выгружаемые вместе с ним.
        /// </summary>
        public List<AttributeExportDTO> Attributes { get; set; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeaveExportDTO" />.
        /// </summary>
        public TreeLeaveExportDTO()
        {
        }

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
            StringValue = leave.StringValue;
            OwningNodeName = leave.ParentNode?.Name ?? "Неизвестный";
            Attributes = leave.Attributes?.Select(a => new AttributeExportDTO(a)).ToList() ?? new();
        }
    }
}
