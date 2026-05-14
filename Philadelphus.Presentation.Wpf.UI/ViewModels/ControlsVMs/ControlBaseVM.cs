using AutoMapper;
using Philadelphus.Core.Domain.Services.Interfaces;
using Serilog;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для элемента управления.
    /// </summary>
    public class ControlBaseVM : ViewModelBase
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;
        protected readonly INotificationService _notificationService;
        protected readonly ApplicationCommandsVM _applicationCommandsVM;
       
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ControlBaseVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ControlBaseVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(applicationCommandsVM);

            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _applicationCommandsVM = applicationCommandsVM;
        }
    }
}
