namespace Philadelphus.Core.Domain.ImportExport.Entities
{
    /// <summary>
    /// Описание доступного адаптера импорта-экспорта.
    /// </summary>
    public class ImportExportAdapterInfo
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportAdapterInfo" />.
        /// </summary>
        /// <param name="fileFormat">Формат файла, с которым работает адаптер.</param>
        /// <param name="adapterName">Наименование адаптера.</param>
        /// <param name="canImport">Признак поддержки импорта из файла.</param>
        /// <param name="canExport">Признак поддержки экспорта в файл.</param>
        public ImportExportAdapterInfo(
            string fileFormat,
            string adapterName,
            bool canImport,
            bool canExport)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileFormat);
            ArgumentException.ThrowIfNullOrWhiteSpace(adapterName);

            FileFormat = fileFormat;
            AdapterName = adapterName;
            CanImport = canImport;
            CanExport = canExport;
        }

        /// <summary>
        /// Формат файла, с которым работает адаптер.
        /// </summary>
        public string FileFormat { get; }

        /// <summary>
        /// Наименование адаптера.
        /// </summary>
        public string AdapterName { get; }

        /// <summary>
        /// Признак поддержки импорта из файла.
        /// </summary>
        public bool CanImport { get; }

        /// <summary>
        /// Признак поддержки экспорта в файл.
        /// </summary>
        public bool CanExport { get; }
    }
}
