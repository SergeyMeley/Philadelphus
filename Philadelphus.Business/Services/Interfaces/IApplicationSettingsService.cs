using Philadelphus.Business.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services.Interfaces
{
    public interface IApplicationSettingsService
    {
        public DirectoryInfo MainConfigDirectory { get; set; }

    }
}
