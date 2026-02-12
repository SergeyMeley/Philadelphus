using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Xunit;
using FluentAssertions;

namespace Philadelphus.Tests.Domain.Entities.MainEntities
{
    public abstract class MainEntityBaseModelTests
    {
        protected abstract MainEntityBaseModel CreateSut(Guid uuid, IMainEntity dbEntity);

        [Fact]
        public void Constructor_ValidArgs_SetsUuidAndDefaultNameAndStateInitialized()
        {
            // Arrange
            var uuid = Guid.NewGuid();
            var mockDbEntity = new Mock<IMainEntity>();

            // Act
            var sut = CreateSut(uuid, mockDbEntity.Object);

            // Assert
            sut.Uuid.Should().Be(uuid);
            sut.Name.Should().NotBeNullOrEmpty();
            sut.Name.Should().StartWith("Новая основная сущность");
            sut.State.Should().Be(State.Initialized);
            sut.AuditInfo.Should().NotBeNull();
            sut.IsHidden.Should().BeFalse();
        }

        [Fact]
        public void Constructor_InvalidUuid_ThrowsArgumentNullException()
        {
            // Arrange
            var mockDbEntity = new Mock<IMainEntity>();
            var act = () => CreateSut(Guid.Empty, mockDbEntity.Object);

            // Act & Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("uuid");
        }

        [Fact]
        public void Constructor_NullDbEntity_ThrowsArgumentNullException()
        {
            // Arrange
            var act = () => CreateSut(Guid.NewGuid(), null!);

            // Act & Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dbEntity");
        }

        [Theory]
        [InlineData(null, "old")]
        [InlineData("new", null)]
        public void Name_SetDifferentValue_UpdatesStateToChanged(string oldValue, string newValue)
        {
            // Arrange
            var uuid = Guid.NewGuid();
            var mockDbEntity = new Mock<IMainEntity>();
            var sut = CreateSut(uuid, mockDbEntity.Object);
            sut.Name = oldValue ?? "";

            // Act
            sut.Name = newValue ?? "";

            // Assert
            sut.Name.Should().Be(newValue ?? "");
            sut.State.Should().Be(State.Changed);
        }

        [Fact]
        public void IsHidden_SetTrue_UpdatesStateToChanged()
        {
            // Arrange
            var sut = CreateSut(Guid.NewGuid(), new Mock<IMainEntity>().Object);

            // Act
            sut.IsHidden = true;

            // Assert
            sut.IsHidden.Should().BeTrue();
            sut.State.Should().Be(State.Changed);
        }
    }
}
