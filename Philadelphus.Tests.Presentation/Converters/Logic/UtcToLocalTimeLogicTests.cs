using FluentAssertions;
using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class UtcToLocalTimeLogicTests
    {
        [Fact]
        public void Convert_Iso8601Utc_Is_Timezone_Independent()
        {
            var utc = new DateTime(2026, 4, 2, 15, 30, 45, DateTimeKind.Utc);

            UtcToLocalTimeLogic.Convert(utc, "Iso8601Utc").Should().Be("2026-04-02T15:30:45Z");
        }

        [Fact]
        public void Convert_NonDateTime_Returns_Same_Value()
            => UtcToLocalTimeLogic.Convert("not a date", "Iso8601").Should().Be("not a date");
    }
}
