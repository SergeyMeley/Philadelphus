using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    /// <summary>
    /// Модель представления операции импорта, экспорта или конвертации.
    /// </summary>
    public class ImportExportOperationVM : ViewModelBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportOperationVM" />.
        /// </summary>
        /// <param name="label">Текст кнопки операции.</param>
        /// <param name="command">Команда операции.</param>
        public ImportExportOperationVM(string label, RelayCommand command)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(label);
            ArgumentNullException.ThrowIfNull(command);

            Label = label;
            Command = command;
        }

        /// <summary>
        /// Текст кнопки операции.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Команда операции.
        /// </summary>
        public RelayCommand Command { get; }
    }
}
