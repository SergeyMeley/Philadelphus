using Philadelphus.Core.Domain.ImportExport.Entities;
using Philadelphus.Presentation.Wpf.UI.ViewModels;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    /// <summary>
    /// Модель представления адаптера импорта-экспорта.
    /// </summary>
    public class ImportExportAdapterVM : ViewModelBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportAdapterVM" />.
        /// </summary>
        /// <param name="adapterInfo">Описание адаптера импорта-экспорта.</param>
        public ImportExportAdapterVM(ImportExportAdapterInfo adapterInfo)
        {
            ArgumentNullException.ThrowIfNull(adapterInfo);

            FileFormat = adapterInfo.FileFormat;
            AdapterName = adapterInfo.AdapterName;
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
        /// Отображаемое наименование адаптера.
        /// </summary>
        public string DisplayName => $"{FileFormat} ({AdapterName})";
    }
}
