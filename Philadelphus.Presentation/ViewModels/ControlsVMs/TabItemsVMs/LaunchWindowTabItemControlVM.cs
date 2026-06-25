using AutoMapper;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Enums;
using Philadelphus.Presentation.Services.Interfaces;
using Serilog;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs.TabItemsVMs
{
    /// <summary>
    /// Модель представления для стартового окна.
    /// </summary>
    public class LaunchWindowTabItemControlVM : TabItemControlBaseVM
    {
        /// <summary>
        /// Иконка таба.
        /// </summary>
        public AppIcon Icon { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LaunchWindowTabItemControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        public LaunchWindowTabItemControlVM(IServiceProvider serviceProvider, 
            IMapper mapper, 
            ILogger logger, 
            INotificationService notificationService, 
            IApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
        }
    }
}
