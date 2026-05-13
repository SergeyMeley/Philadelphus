using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Helpers;
using Philadelphus.Core.Domain.TablesExport.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Philadelphus.Core.Domain.TablesExport.Services
{
    /// <summary>
    /// Сервис экспорта таблиц в файл
    /// </summary>
    public sealed class XmlTablesExportService : ITablesExportService
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Наименование формата
        /// </summary>
        public string FormatName => "XML";

        /// <summary>
        /// Расширение файла
        /// </summary>
        public string FileExtension => "xml";

        /// <summary>
        /// Сервис экспорта таблиц в файл
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений</param>
        public XmlTablesExportService(
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

            var settings = new XmlWriterSettings
            {
                Async = true,
                Indent = true,
                Encoding = System.Text.Encoding.UTF8
            };

            await using var stream = File.Create(path);
            await using var writer = XmlWriter.Create(stream, settings);

            await writer.WriteStartDocumentAsync();

            await writer.WriteStartElementAsync(null, "Report", null);
            await writer.WriteAttributeStringAsync(null, "name", null, reportName);

            await writer.WriteStartElementAsync(null, "Rows", null);

            await foreach (var item in data.WithCancellation(cancellationToken))
            {
                await writer.WriteStartElementAsync(null, "Row", null);

                foreach (var column in columns)
                {
                    var elementName = ToValidXmlElementName(column.Header);
                    var value = column.ValueSelector(item);

                    await writer.WriteStartElementAsync(null, elementName, null);
                    await writer.WriteAttributeStringAsync(null, "header", null, column.Header);

                    if (value != null && value != DBNull.Value)
                    {
                        await writer.WriteStringAsync(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                    }

                    await writer.WriteEndElementAsync();
                }

                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();    // Rows
            await writer.WriteEndElementAsync();    // Report

            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

            _notificationService.SendTextMessage<ITablesExportService>(
                $"Экспорт отчета '{reportName}' успешно выполнен. Сохранено в '{path}'",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return path;
        }

        private static string ToValidXmlElementName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Column";

            var result = new string(value
                .Select(ch => char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_')
                .ToArray());

            if (!char.IsLetter(result[0]) && result[0] != '_')
                result = "_" + result;

            return result;
        }
    }
}
