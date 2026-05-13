namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Метаданные расширения (реализация)
    /// </summary>
    public class ExtensionMetadataModel : IExtensionMetadataModel
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Версия.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Признак автоматического запуска.
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExtensionMetadataModel" />.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="name">Наименование.</param>
        /// <param name="description">Описание.</param>
        /// <param name="version">Версия.</param>
        /// <param name="autoStart">Признак автоматического запуска.</param>
        public ExtensionMetadataModel(
            string id, 
            string name,
            string description,
            string version,
            bool autoStart = false)
        {
            Id = id;
            Name = name;
            Description = description;
            Version = version;
            AutoStart = autoStart;
        }
    }
}
