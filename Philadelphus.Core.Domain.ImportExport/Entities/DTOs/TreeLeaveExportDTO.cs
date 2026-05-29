using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

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
        /// Атрибуты листа.
        /// </summary>
        public List<AttributeExportDTO> Attributes { get; set; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeaveExportDTO" />.
        /// </summary>
        public TreeLeaveExportDTO()
        {
        }
    }
}
