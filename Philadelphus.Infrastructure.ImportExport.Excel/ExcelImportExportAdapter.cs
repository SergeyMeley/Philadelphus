using Philadelphus.Core.Domain.ImportExport.Contracts;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    /// <summary>
    /// Адаптер импорта рабочих деревьев из файлов Excel.
    /// </summary>
    public class ExcelImportExportAdapter : ImportExportAdapterBase
    {
        /// <summary>
        /// Расширение файлов Excel, поддерживаемое адаптером.
        /// </summary>
        public const string XlsxFileFormat = ".xlsx";

        /// <summary>
        /// Признак поддержки экспорта в файл.
        /// </summary>
        public override bool CanExport => false;

        private readonly ConversionService _conversionService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportExportAdapter" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="conversionService">Сервис преобразования Excel-файла в DTO рабочего дерева.</param>
        public ExcelImportExportAdapter(
            INotificationService notificationService,
            ConversionService conversionService)
            : base(notificationService, XlsxFileFormat, "Базовый")
        {
            ArgumentNullException.ThrowIfNull(conversionService);

            _conversionService = conversionService;
        }

        /// <summary>
        /// Сериализует DTO рабочего дерева в файл Excel.
        /// </summary>
        /// <param name="dto">DTO рабочего дерева.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        /// <exception cref="NotSupportedException">Экспорт в Excel пока не поддерживается.</exception>
        public override void Serialize(WorkingTreeExportDTO dto, string filePath)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            throw new NotSupportedException("Экспорт рабочего дерева в Excel пока не поддерживается.");
        }

        /// <summary>
        /// Читает файл Excel и преобразует его в DTO рабочего дерева.
        /// </summary>
        /// <param name="filePath">Путь к исходному файлу Excel.</param>
        /// <returns>DTO рабочего дерева.</returns>
        public override WorkingTreeExportDTO Parse(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            EnsureFileExists(filePath);

            var rootName = Path.GetFileNameWithoutExtension(filePath);
            var json = _conversionService.ProcessFile(filePath, true, rootName);

            return ParseDtoFromJson(json);
        }

        /// <summary>
        /// Преобразует настроенную схему импорта Excel в DTO рабочего дерева.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>DTO рабочего дерева.</returns>
        public WorkingTreeExportDTO Parse(ExcelImportSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema);

            var json = _conversionService.ProcessSchema(schema);

            return ParseDtoFromJson(json);
        }

        private static WorkingTreeExportDTO ParseDtoFromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                return JsonSerializer.Deserialize<WorkingTreeExportDTO>(json, CreateJsonOptions())
                    ?? throw new InvalidOperationException("Excel-импорт не сформировал данные рабочего дерева.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Не удалось прочитать результат Excel-импорта: {ex.Message}", ex);
            }
        }

        private static void EnsureFileExists(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                throw new FileNotFoundException("Файл Excel для импорта не найден.", filePath);
            }
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }
    }
}
