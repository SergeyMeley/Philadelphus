using FluentAssertions;
using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class SelectedIndexLogicTests
    {
        [Theory]
        [InlineData(0, "1")]
        [InlineData(4, "5")]
        [InlineData(41, "42")]
        public void Convert_Returns_OneBased_Index(int index, string expected)
            => SelectedIndexLogic.Convert(index).Should().Be(expected);

        [Fact]
        public void Convert_NonInt_Throws_InvalidCast()
        {
            Action act = () => SelectedIndexLogic.Convert("not an int");

            act.Should().Throw<InvalidCastException>();
        }
    }
}
