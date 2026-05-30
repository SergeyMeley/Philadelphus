namespace Philadelphus.Core.Domain.ImportExport.Entities.DTOs
{
    /// <summary>
    /// DTO для передачи данных рабочего дерева.
    /// </summary>
    public class WorkingTreeExportDTO
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Порядковый номер.
        /// </summary>
        public long Sequence { get; set; }

        /// <summary>
        /// Корень содержимого.
        /// </summary>
        public TreeRootExportDTO ContentRoot { get; set; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WorkingTreeExportDTO" />.
        /// </summary>
        public WorkingTreeExportDTO()
        {
        }
    }
}
