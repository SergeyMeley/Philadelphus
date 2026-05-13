using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Enums;
using Philadelphus.Core.Domain.TablesExport.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.TablesExport.Factories
{
    /// <summary>
    /// Фабрика создания экспорта таблиц.
    /// </summary>
    public sealed class TablesExportServiceFactory : ITablesExportServiceFactory
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TablesExportServiceFactory" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public TablesExportServiceFactory(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Создает объект.
        /// </summary>
        /// <param name="format">Формат.</param>
        /// <returns>Созданный объект.</returns>
        public ITablesExportService Create(
            TablesExportFormat format)
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
