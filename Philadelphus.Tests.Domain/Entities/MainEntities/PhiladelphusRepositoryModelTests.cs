using FluentAssertions;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Xunit;

namespace Philadelphus.Tests.Domain.Entities.MainEntities;

public class PhiladelphusRepositoryModelTests
{
    [Fact]
    public void Constructor_ValidArguments_SetsCorrectInitialState()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var dataStorage = CreateFakeDataStorage();
        var dbEntity = new Mock<PhiladelphusRepository>().Object;

        // Act
        var sut = CreateSut(uuid, dataStorage, dbEntity);

        // Assert
        sut.Uuid.Should().Be(uuid);
        sut.Name.Should().StartWith("Новый репозиторий");
        sut.State.Should().Be(State.Initialized);
        sut.OwnDataStorage.Should().BeSameAs(dataStorage);
        sut.DataStorages.Should().Contain(dataStorage);
        sut.ContentShrub.Should().NotBeNull();
        sut.Content.Should().HaveCount(1);
        sut.IsFavorite.Should().BeFalse();
        sut.IsHidden.Should().BeFalse();
    }

    [Fact]
    public void Constructor_NullDataStorage_ThrowsArgumentNullException()
    {
        var act = () => CreateSut(Guid.NewGuid(), null!, new Mock<PhiladelphusRepository>().Object);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("dataStorage");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsFavorite_SetDifferentValue_UpdatesStateToChanged(bool newValue)
    {
        // Arrange
        var sut = CreateSut(Guid.NewGuid(), CreateFakeDataStorage(), new Mock<PhiladelphusRepository>().Object);

        // Act
        sut.IsFavorite = newValue;

        // Assert
        sut.IsFavorite.Should().Be(newValue);
        sut.State.Should().Be(State.Changed);
    }

    [Fact]
    public void OwnDataStorageName_SetNewValue_UpdatesStorageNameAndState()
    {
        // Arrange
        var sut = CreateSut(Guid.NewGuid(), CreateFakeDataStorage(), new Mock<PhiladelphusRepository>().Object);
        const string newName = "Новое хранилище";

        // Act
        sut.OwnDataStorageName = newName;

        // Assert
        sut.OwnDataStorageName.Should().Be(newName);
        sut.State.Should().Be(State.Changed);
    }

    [Fact]
    public void ChangeDataStorage_Always_ThrowsNotImplementedException()
    {
        // Arrange
        var sut = CreateSut(Guid.NewGuid(), CreateFakeDataStorage(), new Mock<PhiladelphusRepository>().Object);
        var newStorage = CreateFakeDataStorage();

        // Act & Assert
        sut.Invoking(x => x.ChangeDataStorage(newStorage))
            .Should().ThrowExactly<NotImplementedException>();
    }

    private static PhiladelphusRepositoryModelTestingFixture CreateSut(Guid uuid, IDataStorageModel dataStorage, PhiladelphusRepository dbEntity) =>
        new(uuid, dataStorage, dbEntity);

    private static IDataStorageModel CreateFakeDataStorage()
    {
        var mock = new Mock<IDataStorageModel>();
        mock.Setup(x => x.Uuid).Returns(Guid.NewGuid());
        mock.Setup(x => x.Name).Returns("TestStorage");
        mock.SetupSet(x => x.Name = It.IsAny<string>()).Verifiable();
        return mock.Object;
    }
}

// FIXTURE для обхода internal конструктора
internal class PhiladelphusRepositoryModelTestingFixture : PhiladelphusRepositoryModel
{
    public PhiladelphusRepositoryModelTestingFixture(Guid uuid, IDataStorageModel dataStorage, PhiladelphusRepository dbEntity)
        : base(uuid, dataStorage, dbEntity) { }

    public override IDataStorageModel DataStorage => OwnDataStorage;
}
