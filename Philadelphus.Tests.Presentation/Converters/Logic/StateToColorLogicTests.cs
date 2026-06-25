using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Enums;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class StateToColorLogicTests
    {
        [Theory]
        [InlineData(State.Initialized, ConverterColor.DeepPink)]
        [InlineData(State.Changed, ConverterColor.Cyan)]
        [InlineData(State.SavedOrLoaded, ConverterColor.YellowGreen)]
        [InlineData(State.ForSoftDelete, ConverterColor.OrangeRed)]
        [InlineData(State.ForHardDelete, ConverterColor.Red)]
        [InlineData(State.SoftDeleted, ConverterColor.IndianRed)]
        public void ResolveColor_Maps_State(State state, ConverterColor expected)
            => StateToColorLogic.ResolveColor(state).Should().Be(expected);

        [Fact]
        public void ResolveColor_NonState_Is_White()
            => StateToColorLogic.ResolveColor("not a state").Should().Be(ConverterColor.White);

        [Fact]
        public void ResolveColor_Null_Is_White()
            => StateToColorLogic.ResolveColor(null).Should().Be(ConverterColor.White);
    }
}
