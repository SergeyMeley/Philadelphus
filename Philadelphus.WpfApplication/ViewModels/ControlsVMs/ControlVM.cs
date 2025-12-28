using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.ControlsVMs
{
    public class ControlVM : ViewModelBase
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger<ControlVM> _logger;
        protected readonly INotificationService _notificationService;
        protected readonly ApplicationCommandsVM _applicationCommandsVM;
        public ControlVM(
            IServiceProvider serviceProvider,
            ILogger<ControlVM> logger,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _notificationService = notificationService;
            _applicationCommandsVM = applicationCommandsVM;
        }
    }
}
