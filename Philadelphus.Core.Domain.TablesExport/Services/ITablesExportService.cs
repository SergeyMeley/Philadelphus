using Philadelphus.Core.Domain.TablesExport.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.TablesExport.Services
{
    /// <summary>
    /// Сервис экспорта таблиц в файл
    /// </summary>
    public interface ITablesExportService
    {
        /// <summary>
        /// Наименование формата
        /// </summary>
        string FormatName { get; }

        /// <summary>
        /// Расширение файла
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Экспортировать таблицу в файл
        /// </summary>
        /// <typeparam name="T">Тип данных строки</typeparam>
        /// <param name="data">Данные для экспорта</param>
        /// <param name="columns">Колонки таблицы</param>
        /// <param name="reportName">Наименование отчета</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns></returns>
        Task<string> ExportAsync<T>(
            IAsyncEnumerable<T> data,
            IReadOnlyList<TableExportColumn<T>> columns,
            string reportName,
            CancellationToken cancellationToken = default);
    }
}
