using Philadelphus.Business.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services.Implementations
{
    public class ApplicationSettingsService : IApplicationSettingsService
    {
        public DirectoryInfo MainConfigDirectory { get; set; }
    }
}
