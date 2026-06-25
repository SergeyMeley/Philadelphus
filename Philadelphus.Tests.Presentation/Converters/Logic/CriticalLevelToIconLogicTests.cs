using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Enums;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class CriticalLevelToIconLogicTests
    {
        [Theory]
        [InlineData(NotificationCriticalLevelModel.None, AppIcon.Empty)]
        [InlineData(NotificationCriticalLevelModel.Ok, AppIcon.StatusOk)]
        [InlineData(NotificationCriticalLevelModel.Info, AppIcon.StatusInfo)]
        [InlineData(NotificationCriticalLevelModel.Warning, AppIcon.StatusWarning)]
        [InlineData(NotificationCriticalLevelModel.Error, AppIcon.StatusError)]
        [InlineData(NotificationCriticalLevelModel.Alarm, AppIcon.StatusAlarm)]
        public void ResolveIcon_Maps_CriticalLevel(NotificationCriticalLevelModel level, AppIcon expected)
            => CriticalLevelToIconLogic.ResolveIcon(level).Should().Be(expected);

        [Fact]
        public void ResolveIcon_NonLevel_Is_Empty()
            => CriticalLevelToIconLogic.ResolveIcon("not a level").Should().Be(AppIcon.Empty);
    }
}
