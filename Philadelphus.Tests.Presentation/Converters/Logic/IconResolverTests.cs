using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Enums;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class IconResolverTests
    {
        [Fact]
        public void Resolve_Returns_Same_AppIcon_When_Already_AppIcon()
            => IconResolver.Resolve(AppIcon.Settings).Should().Be(AppIcon.Settings);

        [Theory]
        [InlineData(NotificationCriticalLevelModel.Ok, AppIcon.StatusOk)]
        [InlineData(NotificationCriticalLevelModel.Warning, AppIcon.StatusWarning)]
        [InlineData(NotificationCriticalLevelModel.Error, AppIcon.StatusError)]
        [InlineData(NotificationCriticalLevelModel.None, AppIcon.Empty)]
        public void Resolve_Maps_CriticalLevel_To_Status_Icon(NotificationCriticalLevelModel level, AppIcon expected)
            => IconResolver.Resolve(level).Should().Be(expected);

        [Fact]
        public void Resolve_Null_Falls_Back_To_Empty()
            => IconResolver.Resolve(null).Should().Be(AppIcon.Empty);

        [Fact]
        public void Resolve_Unknown_Type_Falls_Back_To_Empty()
            => IconResolver.Resolve("whatever").Should().Be(AppIcon.Empty);

        [Fact]
        public void Resolve_Unmapped_Entity_Falls_Back_To_Empty()
            => IconResolver.Resolve(new FakeMainEntityVM()).Should().Be(AppIcon.Empty);

        private sealed class FakeMainEntityVM : IMainEntityVM
        {
            public bool IsTreeExpanded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
