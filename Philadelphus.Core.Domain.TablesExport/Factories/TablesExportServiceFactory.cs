using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Enums;
using Philadelphus.Core.Domain.TablesExport.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.TablesExport.Factories
{
    public sealed class TablesExportServiceFactory : ITablesExportServiceFactory
    {
        private readonly INotificationService _notificationService;

        public TablesExportServiceFactory(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public ITablesExportService Create(TablesExportFormat format)
        {
            switch (format)
            {
                case TablesExportFormat.Xlsx:
                    return new OpenXmlExcelTablesExportService(_notificationService);
                    break;
                case TablesExportFormat.Json:
                    return new JsonTablesExportService(_notificationService);
                    break;
                case TablesExportFormat.Xml:
                    return new XmlTablesExportService(_notificationService);
                    break;
                default:
                    throw new Exception($"Не найден сервис экспорта в {format}");
                    break;
            }
        }
    }
}
