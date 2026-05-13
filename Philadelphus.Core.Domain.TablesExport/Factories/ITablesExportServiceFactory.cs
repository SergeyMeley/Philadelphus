using Philadelphus.Core.Domain.TablesExport.Enums;
using Philadelphus.Core.Domain.TablesExport.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.TablesExport.Factories
{
    /// <summary>
    /// Задает контракт для работы с экспорта таблиц.
    /// </summary>
    public interface ITablesExportServiceFactory
    {
        ITablesExportService Create(TablesExportFormat format);
    }
}
