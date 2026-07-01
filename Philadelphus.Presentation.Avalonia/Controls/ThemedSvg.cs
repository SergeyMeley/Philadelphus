using global::Avalonia;
using global::Avalonia.Styling;

namespace Philadelphus.Presentation.Avalonia.Controls
{
    public sealed class ThemedSvg : global::Avalonia.Svg.Skia.Svg
    {
        private const string DefaultLightFolder = "/Assets/Icons/svg/light";
        private const string DefaultDarkFolder = "/Assets/Icons/svg/dark";

        public ThemedSvg(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public static readonly StyledProperty<string?> IconProperty =
            AvaloniaProperty.Register<ThemedSvg, string?>(nameof(Icon));

        public static readonly StyledProperty<string> LightFolderProperty =
            AvaloniaProperty.Register<ThemedSvg, string>(nameof(LightFolder), DefaultLightFolder);

        public static readonly StyledProperty<string> DarkFolderProperty =
            AvaloniaProperty.Register<ThemedSvg, string>(nameof(DarkFolder), DefaultDarkFolder);

        public string? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string LightFolder
        {
            get => GetValue(LightFolderProperty);
            set => SetValue(LightFolderProperty, value);
        }

        public string DarkFolder
        {
            get => GetValue(DarkFolderProperty);
            set => SetValue(DarkFolderProperty, value);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            UpdatePath();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IconProperty
                || change.Property == LightFolderProperty
                || change.Property == DarkFolderProperty
                || change.Property.Name == nameof(ActualThemeVariant))
            {
                UpdatePath();
            }
        }

        private void UpdatePath()
        {
            if (string.IsNullOrWhiteSpace(Icon))
            {
                Path = null;
                return;
            }

            var folder = ActualThemeVariant == ThemeVariant.Dark
                ? DarkFolder
                : LightFolder;

            Path = $"{folder.TrimEnd('/')}/{Icon}.svg";
        }
    }
}
