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
        public ImportExportAdapterInfo(string fileFormat, string adapterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileFormat);
            ArgumentException.ThrowIfNullOrWhiteSpace(adapterName);

            FileFormat = fileFormat;
            AdapterName = adapterName;
        }

        /// <summary>
        /// Формат файла, с которым работает адаптер.
        /// </summary>
        public string FileFormat { get; }

        /// <summary>
        /// Наименование адаптера.
        /// </summary>
        public string AdapterName { get; }
    }
}
