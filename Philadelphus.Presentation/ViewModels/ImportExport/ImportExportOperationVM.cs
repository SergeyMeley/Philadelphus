using IRelayCommand = Philadelphus.Presentation.Infrastructure.IRelayCommand;
using Philadelphus.Presentation.ViewModels;

namespace Philadelphus.Presentation.ViewModels.ImportExport
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
        public ImportExportOperationVM(string label, IRelayCommand command)
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
        public IRelayCommand Command { get; }
    }
}
