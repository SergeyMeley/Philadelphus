using Philadelphus.Core.Domain.ImportExport.Entities.DTOs.ImportExportDTOs;
using Philadelphus.Core.Domain.ImportExport.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.ImportExport.Implementations
{
    /// <summary>
    /// Базовая реализация адаптера импорта и экспорта.
    /// </summary>
    public abstract class ImportExportAdapterBase : IImportExportAdapter
    {
        /// <summary>
        /// Максимальная длина наименования адаптера.
        /// </summary>
        public const int MaxAdapterNameLength = 20;

        private readonly INotificationService _notificationService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportAdapterBase" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="fileFormat">Формат файла, с которым работает адаптер.</param>
        /// <param name="adapterName">Наименование адаптера.</param>
        protected ImportExportAdapterBase(
            INotificationService notificationService,
            string fileFormat,
            string adapterName)
        {
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileFormat);
            ValidateAdapterName(adapterName);

            _notificationService = notificationService;
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

        /// <summary>
        /// Сервис уведомлений.
        /// </summary>
        protected INotificationService NotificationService => _notificationService;

        /// <summary>
        /// Сериализует DTO импорта-экспорта в файл поддерживаемого формата.
        /// </summary>
        /// <param name="dto">DTO рабочего дерева.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        public abstract void Serialize(WorkingTreeExportDTO dto, string filePath);

        /// <summary>
        /// Читает файл поддерживаемого формата и преобразует его в DTO импорта-экспорта.
        /// </summary>
        /// <param name="filePath">Путь к исходному файлу.</param>
        /// <returns>DTO рабочего дерева.</returns>
        public abstract WorkingTreeExportDTO Parse(string filePath);

        /// <summary>
        /// Проверяет наименование адаптера.
        /// </summary>
        /// <param name="adapterName">Наименование адаптера.</param>
        protected static void ValidateAdapterName(string adapterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(adapterName);

            if (adapterName.Length > MaxAdapterNameLength)
            {
                throw new ArgumentException(
                    $"Длина наименования адаптера должна быть не более {MaxAdapterNameLength} символов.",
                    nameof(adapterName));
            }
        }
    }
}
