using Microsoft.Extensions.Logging;
using Philadelphus.Business.Services.Interfaces;
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
        protected readonly ILogger<RepositoryCreationControlVM> _logger;
        protected readonly INotificationService _notificationService;
        public ControlVM(
            IServiceProvider serviceProvider,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _notificationService = notificationService;
        }
    }
}
