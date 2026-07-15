using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Enums;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class StateToColorLogicTests
    {
        [Theory]
        [InlineData(State.Initialized, ConverterColor.StateInitialized)]
        [InlineData(State.Changed, ConverterColor.StateChanged)]
        [InlineData(State.SavedOrLoaded, ConverterColor.StateSavedOrLoaded)]
        [InlineData(State.ForSoftDelete, ConverterColor.StateForSoftDelete)]
        [InlineData(State.ForHardDelete, ConverterColor.StateForHardDelete)]
        [InlineData(State.SoftDeleted, ConverterColor.StateSoftDeleted)]
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
