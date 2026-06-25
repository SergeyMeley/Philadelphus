using FluentAssertions;
using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Enums;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class BooleanToColorLogicTests
    {
        [Theory]
        [InlineData(true, ConverterColor.Green)]
        [InlineData(false, ConverterColor.Red)]
        public void ResolveColor_Maps_Bool_Value(bool value, ConverterColor expected)
            => BooleanToColorLogic.ResolveColor(value).Should().Be(expected);

        [Fact]
        public void ResolveColor_Null_Is_Black()
            => BooleanToColorLogic.ResolveColor(null).Should().Be(ConverterColor.Black);

        [Fact]
        public void ResolveColor_NonBool_Is_DarkRed()
            => BooleanToColorLogic.ResolveColor("not a bool").Should().Be(ConverterColor.DarkRed);
    }
}
