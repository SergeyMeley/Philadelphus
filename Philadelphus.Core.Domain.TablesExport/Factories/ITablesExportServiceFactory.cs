using Philadelphus.Core.Domain.TablesExport.Enums;
using Philadelphus.Core.Domain.TablesExport.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.TablesExport.Factories
{
    public interface ITablesExportServiceFactory
    {
        ITablesExportService Create(TablesExportFormat format);
    }
}
