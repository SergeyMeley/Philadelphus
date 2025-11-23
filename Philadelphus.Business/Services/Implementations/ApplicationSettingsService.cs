using Microsoft.Extensions.Configuration;
using Philadelphus.Business.Config;
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
        private readonly ApplicationSettings _settings;
        public ApplicationSettingsService(ApplicationSettings settings)
        {
            _settings = settings;
        }
        public ApplicationSettings GetSettings() => _settings;
    }
}
