using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.TabItemsVMs
{
    public class LaunchWindowTabItemVM : TabItemControlBaseVM
    {
        public string IconKey { get; set; }
        public LaunchWindowTabItemVM(IServiceProvider serviceProvider, 
            IMapper mapper, 
            ILogger<ControlBaseVM> logger, 
            INotificationService notificationService, 
            ApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
        }
    }
}
