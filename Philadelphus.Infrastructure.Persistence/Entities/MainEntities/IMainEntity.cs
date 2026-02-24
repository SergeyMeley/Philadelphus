using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    public interface IMainEntity
    {
        public Guid Uuid { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }
        public long? Sequence { get; set; }
        public string? Alias { get; set; }
        public string? CustomCode { get; set; }

        /// <summary>
        /// Скрыт от нового использования (устаревший для бизнеса)
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        public AuditInfo AuditInfo { get; set; }
    }
}
