using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.TablesExport.Models
{
    public sealed class TableExportColumn<T>
    {
        public required string Header { get; init; }
        public required Func<T, object?> ValueSelector { get; init; }
        public double Width { get; init; } = 20;
    }
}
