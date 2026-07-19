using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System.Text.Json.Serialization;

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
        /// Порядковый номер.
        /// </summary>
        public long Sequence { get; set; }

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
        /// Временный идентификатор строки внутри одной операции импорта.
        /// </summary>
        /// <remarks>
        /// Используется только для связывания уже созданной модели с исходной строкой
        /// и не попадает в обычный JSON-экспорт рабочего дерева.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? ImportCorrelationId { get; set; }

        /// <summary>
        /// Временный идентификатор строки полиморфного родителя внутри операции импорта.
        /// </summary>
        /// <remarks>
        /// Значение формируется из настроенного Excel FK и не требует знать имя
        /// создаваемого родительского листа.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? PolymorphicParentImportCorrelationId { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeaveExportDTO" />.
        /// </summary>
        public TreeLeaveExportDTO()
        {
        }
    }
}
