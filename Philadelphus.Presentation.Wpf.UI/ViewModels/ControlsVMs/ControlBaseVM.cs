using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class ControlBaseVM : ViewModelBase
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IMapper _mapper;
        protected readonly ILogger<ControlBaseVM> _logger;
        protected readonly INotificationService _notificationService;
        protected readonly ApplicationCommandsVM _applicationCommandsVM;
        public ControlBaseVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<ControlBaseVM> logger,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM)
        {
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _applicationCommandsVM = applicationCommandsVM;
        }
    }
}
