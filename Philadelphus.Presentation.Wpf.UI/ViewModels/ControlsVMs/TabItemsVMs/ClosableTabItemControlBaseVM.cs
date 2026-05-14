using AutoMapper;
using Philadelphus.Core.Domain.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.TabItemsVMs
{
    /// <summary>
    /// Модель представления для элемента управления.
    /// </summary>
    public class ClosableTabItemControlBaseVM : TabItemControlBaseVM
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ClosableTabItemControlBaseVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        public ClosableTabItemControlBaseVM(IServiceProvider serviceProvider, 
            IMapper mapper, 
            ILogger logger, 
            INotificationService notificationService, 
            ApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
        }
    }
}
