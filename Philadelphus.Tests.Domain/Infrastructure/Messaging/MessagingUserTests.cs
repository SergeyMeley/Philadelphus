using FluentAssertions;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using System.Text.Json;

namespace Philadelphus.Tests.Domain.Infrastructure.Messaging
{
    public class MessagingUserTests
    {
        [Fact]
        public void UserName_WhenManualNameIsSet_ContainsManualAndAutomaticNames()
        {
            var user = new MessagingUser(
                Guid.CreateVersion7(),
                "Operator",
                "DOMAIN\\auto-user");

            user.DisplayUserName.Should().Be("Operator");
            user.UserName.Should().Be("Operator [DOMAIN\\auto-user]");
        }

        [Fact]
        public void UserName_WhenManualNameIsEmpty_UsesAutomaticNameOnly()
        {
            var user = new MessagingUser(
                Guid.CreateVersion7(),
                " ",
                "DOMAIN\\auto-user");

            user.DisplayUserName.Should().Be("DOMAIN\\auto-user");
            user.UserName.Should().Be("DOMAIN\\auto-user");
        }

        [Fact]
        public void Serialization_WhenManualNameIsSet_PreservesManualAndAutomaticNames()
        {
            var user = new MessagingUser(
                Guid.CreateVersion7(),
                "Operator",
                "DOMAIN\\auto-user");

            var json = JsonSerializer.Serialize(user);
            var restoredUser = JsonSerializer.Deserialize<MessagingUser>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            restoredUser.Should().NotBeNull();
            restoredUser!.ManualUserName.Should().Be("Operator");
            restoredUser.AutomaticUserName.Should().Be("DOMAIN\\auto-user");
            restoredUser.UserName.Should().Be("Operator [DOMAIN\\auto-user]");
        }

        [Fact]
        public void Serialization_WhenOnlyLegacyUserNameExists_UsesItAsAutomaticName()
        {
            var userUuid = Guid.CreateVersion7();
            var json = $$"""{"UserUuid":"{{userUuid}}","UserName":"legacy-user"}""";

            var restoredUser = JsonSerializer.Deserialize<MessagingUser>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            restoredUser.Should().NotBeNull();
            restoredUser!.ManualUserName.Should().BeNull();
            restoredUser.AutomaticUserName.Should().Be("legacy-user");
            restoredUser.UserName.Should().Be("legacy-user");
        }
    }
}
