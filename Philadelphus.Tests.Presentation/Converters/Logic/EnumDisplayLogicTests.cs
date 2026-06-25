using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Tests.Presentation.Converters.Logic
{
    public class EnumDisplayLogicTests
    {
        [Fact]
        public void Convert_Enum_Returns_DisplayName()
            => EnumDisplayLogic.Convert(NotificationTransmissionType.Self, null).Should().Be("Себе");

        [Fact]
        public void Convert_Enum_Broadcast_Returns_DisplayName()
            => EnumDisplayLogic.Convert(NotificationTransmissionType.Broadcast, null).Should().Be("Всем");

        [Fact]
        public void Convert_Description_Param_Falls_Back_To_Value_When_No_Description()
            => EnumDisplayLogic.Convert(NotificationTransmissionType.Self, "Description").Should().Be("Self");

        [Fact]
        public void Convert_NonEnum_Returns_ToString()
            => EnumDisplayLogic.Convert("plain text", null).Should().Be("plain text");

        [Fact]
        public void Convert_Null_Returns_Null()
            => EnumDisplayLogic.Convert(null, null).Should().BeNull();
    }
}
