using global::Avalonia;
using global::Avalonia.Media;
using global::Avalonia.Styling;

using Philadelphus.Presentation.Enums;
using Philadelphus.Presentation.Theming;

namespace Philadelphus.Presentation.Avalonia.Helpers
{
    /// <summary>
    /// Материализация <see cref="ConverterColor" /> в Avalonia-кисть (кэшированные <see cref="Brushes" />).
    /// </summary>
    internal static class ConverterColorBrushes
    {
        private static readonly Dictionary<ConverterColor, SolidColorBrush> StateBrushes = new()
        {
            [ConverterColor.StateInitialized] = new(Colors.Transparent),
            [ConverterColor.StateChanged] = new(Colors.Transparent),
            [ConverterColor.StateSavedOrLoaded] = new(Colors.Transparent),
            [ConverterColor.StateForSoftDelete] = new(Colors.Transparent),
            [ConverterColor.StateForHardDelete] = new(Colors.Transparent),
            [ConverterColor.StateSoftDeleted] = new(Colors.Transparent),
        };

        private static Application? _subscribedApplication;
        private static bool? _isDarkTheme;

        public static IBrush ToBrush(ConverterColor color)
        {
            EnsureStatePalette();

            if (StateBrushes.TryGetValue(color, out var stateBrush))
            {
                return stateBrush;
            }

            return color switch
            {
                ConverterColor.Black => Brushes.Black,
                ConverterColor.White => Brushes.White,
                ConverterColor.Green => Brushes.Green,
                ConverterColor.Red => Brushes.Red,
                ConverterColor.DarkRed => Brushes.DarkRed,
                ConverterColor.IndianRed => Brushes.IndianRed,
                ConverterColor.OrangeRed => Brushes.OrangeRed,
                ConverterColor.DeepPink => Brushes.DeepPink,
                ConverterColor.Cyan => Brushes.Cyan,
                ConverterColor.YellowGreen => Brushes.YellowGreen,
                _ => Brushes.Transparent
            };
        }

        private static void EnsureStatePalette()
        {
            var application = Application.Current;
            if (application != null && ReferenceEquals(application, _subscribedApplication) == false)
            {
                if (_subscribedApplication != null)
                {
                    _subscribedApplication.PropertyChanged -= OnApplicationPropertyChanged;
                }

                _subscribedApplication = application;
                application.PropertyChanged += OnApplicationPropertyChanged;
            }

            ApplyStatePalette(application?.ActualThemeVariant == ThemeVariant.Dark);
        }

        private static void OnApplicationPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == Application.ActualThemeVariantProperty && sender is Application application)
            {
                ApplyStatePalette(application.ActualThemeVariant == ThemeVariant.Dark);
            }
        }

        private static void ApplyStatePalette(bool isDarkTheme)
        {
            if (_isDarkTheme == isDarkTheme)
            {
                return;
            }

            foreach (var (role, brush) in StateBrushes)
            {
                brush.Color = Color.Parse(StateColorPalette.ResolveHex(role, isDarkTheme));
            }

            _isDarkTheme = isDarkTheme;
        }
    }
}
