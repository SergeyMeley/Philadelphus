using Philadelphus.Core.Domain.ImportExport.Contracts;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Infrastructure.ImportExport.Phjson
{
    /// <summary>
    /// Адаптер импорта и экспорта рабочих деревьев в формате PHJSON.
    /// </summary>
    public class JsonImportExportAdapter : ImportExportAdapterBase
    {
        /// <summary>
        /// Расширение файлов PHJSON.
        /// </summary>
        public const string PhjsonFileFormat = ".phjson";

        private const int MaxJsonSize = 500 * 1024 * 1024;
        private const int MaxJsonDepth = 100;

        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            Converters = { new JsonStringEnumConverter() },
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonDocumentOptions _documentOptions = new()
        {
            MaxDepth = MaxJsonDepth,
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow
        };

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="JsonImportExportAdapter" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public JsonImportExportAdapter(INotificationService notificationService)
            : base(notificationService, PhjsonFileFormat, "Базовый")
        {
        }

        /// <summary>
        /// Сериализует DTO рабочего дерева в файл PHJSON.
        /// </summary>
        /// <param name="dto">DTO рабочего дерева.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        public override void Serialize(WorkingTreeExportDTO dto, string filePath)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            File.WriteAllText(filePath, JsonSerializer.Serialize(dto, _options));
        }

        /// <summary>
        /// Читает файл PHJSON и преобразует его в DTO рабочего дерева.
        /// </summary>
        /// <param name="filePath">Путь к исходному файлу.</param>
        /// <returns>DTO рабочего дерева.</returns>
        public override WorkingTreeExportDTO Parse(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (File.Exists(filePath) == false)
            {
                throw new FileNotFoundException("Файл импорта не найден.", filePath);
            }

            var json = File.ReadAllText(filePath);
            ValidateJsonSize(json);

            try
            {
                using var document = JsonDocument.Parse(json, _documentOptions);

                return JsonSerializer.Deserialize<WorkingTreeExportDTO>(json, _options)
                    ?? throw new InvalidOperationException("JSON не содержит данные рабочего дерева.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Ошибка парсинга JSON: {ex.Message}", ex);
            }
        }

        private static void ValidateJsonSize(string json)
        {
            var jsonSize = Encoding.UTF8.GetByteCount(json);
            if (jsonSize > MaxJsonSize)
            {
                throw new InvalidOperationException(
                    $"JSON слишком большой: {jsonSize} байт (максимум {MaxJsonSize} байт).");
            }
        }
    }
}
