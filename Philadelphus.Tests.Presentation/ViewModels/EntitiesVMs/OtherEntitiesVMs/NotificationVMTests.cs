using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Identity.Entities;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.OtherEntitiesVMs;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Presentation.ViewModels.EntitiesVMs.OtherEntitiesVMs
{
    public class NotificationVMTests
    {
        [Fact]
        public void SenderDisplayName_WhenOtherUserHasSameDisplayName_IncludesAutomaticUserName()
        {
            var userService = new FakeUserService
            {
                CurrentUser = new User(
                    Guid.CreateVersion7(),
                    "Operator",
                    "CLIENT-1\\auto-user")
            };
            var sender = new MessagingUser(
                Guid.CreateVersion7(),
                "Operator",
                "CLIENT-2\\auto-user");
            var notification = new Notification(
                "Message",
                sender,
                "Test.Source",
                NotificationCriticalLevelModel.Info);

            var vm = new NotificationVM(notification, userService);

            vm.SenderDisplayName.Should().StartWith("Operator [CLIENT-2\\auto-user] (");
        }

        [Fact]
        public void SenderDisplayName_WhenOtherUserHasDifferentDisplayName_UsesShortDisplayName()
        {
            var userService = new FakeUserService
            {
                CurrentUser = new User(
                    Guid.CreateVersion7(),
                    "Operator",
                    "CLIENT-1\\auto-user")
            };
            var sender = new MessagingUser(
                Guid.CreateVersion7(),
                "Inspector",
                "CLIENT-2\\auto-user");
            var notification = new Notification(
                "Message",
                sender,
                "Test.Source",
                NotificationCriticalLevelModel.Info);

            var vm = new NotificationVM(notification, userService);

            vm.SenderDisplayName.Should().StartWith("Inspector (");
            vm.SenderDisplayName.Should().NotContain("CLIENT-2\\auto-user");
        }
    }
}
