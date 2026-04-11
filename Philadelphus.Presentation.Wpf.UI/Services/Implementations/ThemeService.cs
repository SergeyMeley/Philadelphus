using Microsoft.Win32;
using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services.Implementations
{
    public class ThemeService : IThemeService
    {
        public void ApplyTheme(ControlsThemeMode mode)
        {
            switch (mode)
            {
                case ControlsThemeMode.Dark:
                    SetTheme("Dark");
                    break;

                case ControlsThemeMode.Light:
                    SetTheme("Light");
                    break;

                case ControlsThemeMode.System:
                    ApplySystemTheme();
                    break;
            }
        }

        private void ApplySystemTheme()
        {
            if (IsSystemDark())
                SetTheme("Dark");
            else
                SetTheme("Light");
        }

        private bool IsSystemDark()
        {
            var value = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme",
                1);

            return (int)value == 0;
        }

        private void SetTheme(string theme)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;

            // ищем текущую тему
            var existingTheme = dictionaries
                .FirstOrDefault(d => d.Source != null &&
                                     d.Source.OriginalString.Contains("Resources/Themes/"));

            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"Resources/Themes/{theme}.xaml", UriKind.Relative)
            };

            if (existingTheme != null)
            {
                var index = dictionaries.IndexOf(existingTheme);
                dictionaries[index] = newTheme;
            }
            else
            {
                dictionaries.Insert(0, newTheme);
            }
        }
    }
}
