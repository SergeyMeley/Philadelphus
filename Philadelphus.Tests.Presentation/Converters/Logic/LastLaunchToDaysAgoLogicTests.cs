using FluentAssertions;
using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class LastLaunchToDaysAgoLogicTests
    {
        [Fact]
        public void Convert_Null_Is_NeverLaunched()
            => LastLaunchToDaysAgoLogic.Convert(null).Should().Be("никогда не запускался");

        [Fact]
        public void Convert_NonDate_Is_NeverLaunched()
            => LastLaunchToDaysAgoLogic.Convert("not a date").Should().Be("никогда не запускался");

        [Fact]
        public void Convert_FutureDate_Is_InFuture()
            => LastLaunchToDaysAgoLogic.Convert(DateTime.Now.AddDays(2)).Should().Be("в будущем");

        [Theory]
        [InlineData(1, "1 день назад")]
        [InlineData(2, "2 дня назад")]
        [InlineData(5, "5 дней назад")]
        [InlineData(11, "11 дней назад")]
        [InlineData(21, "21 день назад")]
        public void Convert_PastDate_Uses_Russian_Plural(int daysAgo, string expected)
            => LastLaunchToDaysAgoLogic.Convert(DateTime.Now.AddDays(-daysAgo - 0.5)).Should().Be(expected);
    }
}
