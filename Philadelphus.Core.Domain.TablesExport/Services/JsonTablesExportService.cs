using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Helpers;
using Philadelphus.Core.Domain.TablesExport.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Philadelphus.Core.Domain.TablesExport.Services
{
    /// <summary>
    /// Сервис экспорта таблиц в файл
    /// </summary>
    public sealed class JsonTablesExportService : ITablesExportService
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Наименование формата
        /// </summary>
        public string FormatName => "JSON";

        /// <summary>
        /// Расширение файла
        /// </summary>
        public string FileExtension => "json";

        /// <summary>
        /// Сервис экспорта таблиц в файл
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public JsonTablesExportService(
            INotificationService notificationService)
        {
            ArgumentNullException.ThrowIfNull(notificationService);

            _notificationService = notificationService;
        }

        /// <summary>
        /// Экспортировать таблицу в файл
        /// </summary>
        /// <typeparam name="T">Тип данных строки</typeparam>
        /// <param name="data">Данные для экспорта</param>
        /// <param name="columns">Колонки таблицы</param>
        /// <param name="reportName">Наименование отчета</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        public async Task<string> ExportAsync<T>(
            IAsyncEnumerable<T> data,
            IReadOnlyList<TableExportColumn<T>> columns,
            string reportName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentException.ThrowIfNullOrWhiteSpace(reportName);

            var path = TablesExportPathBuilder.BuildExportPath(reportName, FileExtension);

            await using var stream = File.Create(path);

            var options = new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            await using var writer = new Utf8JsonWriter(stream, options);

            writer.WriteStartArray();

            await foreach (var item in data.WithCancellation(cancellationToken))
            {
                writer.WriteStartObject();

                foreach (var column in columns)
                {
                    writer.WritePropertyName(column.Header);
                    WriteJsonValue(writer, column.ValueSelector(item));
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            await writer.FlushAsync(cancellationToken);

            _notificationService.SendTextMessage<ITablesExportService>(
                $"Экспорт отчета '{reportName}' успешно выполнен. Сохранено в '{path}'",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return path;
        }

        private static void WriteJsonValue(Utf8JsonWriter writer, object? value)
        {
            switch (value)
            {
                case null:
                case DBNull:
                    writer.WriteNullValue();
                    break;

                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;

                case bool boolValue:
                    writer.WriteBooleanValue(boolValue);
                    break;

                case byte byteValue:
                    writer.WriteNumberValue(byteValue);
                    break;

                case short shortValue:
                    writer.WriteNumberValue(shortValue);
                    break;

                case int intValue:
                    writer.WriteNumberValue(intValue);
                    break;

                case long longValue:
                    writer.WriteNumberValue(longValue);
                    break;

                case float floatValue:
                    writer.WriteNumberValue(floatValue);
                    break;

                case double doubleValue:
                    writer.WriteNumberValue(doubleValue);
                    break;

                case decimal decimalValue:
                    writer.WriteNumberValue(decimalValue);
                    break;

                case DateTime dateTime:
                    writer.WriteStringValue(dateTime);
                    break;

                case DateTimeOffset dateTimeOffset:
                    writer.WriteStringValue(dateTimeOffset);
                    break;

                default:
                    writer.WriteStringValue(value.ToString());
                    break;
            }
        }
    }
}
