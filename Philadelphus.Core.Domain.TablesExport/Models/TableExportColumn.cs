using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.TablesExport.Models
{
    /// <summary>
    /// Представляет объект колонки экспорта таблицы.
    /// </summary>
    public sealed class TableExportColumn<T>
    {
        /// <summary>
        /// Заголовок.
        /// </summary>
        public required string Header { get; init; }

        /// <summary>
        /// Выборщик значений.
        /// </summary>
        public required Func<T, object?> ValueSelector { get; init; }

        /// <summary>
        /// Ширина.
        /// </summary>
        public double Width { get; init; } = 20;
    }
}
