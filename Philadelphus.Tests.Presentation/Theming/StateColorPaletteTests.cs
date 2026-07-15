using FluentAssertions;

using Philadelphus.Presentation.Enums;
using Philadelphus.Presentation.Theming;

namespace Philadelphus.Tests.Presentation.Theming
{
    public class StateColorPaletteTests
    {
        [Theory]
        [InlineData(ConverterColor.StateInitialized, "#1349EC", "#5B7CFA")]
        [InlineData(ConverterColor.StateChanged, "#B82EB8", "#E879D2")]
        [InlineData(ConverterColor.StateSavedOrLoaded, "#169C59", "#5EED9A")]
        [InlineData(ConverterColor.StateForSoftDelete, "#D65F00", "#FF9F43")]
        [InlineData(ConverterColor.StateForHardDelete, "#8E0B16", "#D61F2C")]
        [InlineData(ConverterColor.StateSoftDeleted, "#595959", "#A3A3A3")]
        public void ResolveHex_ReturnsThemeColor(ConverterColor color, string light, string dark)
        {
            StateColorPalette.ResolveHex(color, isDarkTheme: false).Should().Be(light);
            StateColorPalette.ResolveHex(color, isDarkTheme: true).Should().Be(dark);
        }

        [Fact]
        public void ResolveHex_NonStateColor_Throws()
            => FluentActions.Invoking(() => StateColorPalette.ResolveHex(ConverterColor.White, isDarkTheme: false))
                .Should().Throw<ArgumentOutOfRangeException>();
    }
}
