using Microsoft.Win32;
using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services.Interfaces
{
    public interface IThemeService
    {
        public void ApplyTheme(ControlsThemeMode mode);
    }
}
