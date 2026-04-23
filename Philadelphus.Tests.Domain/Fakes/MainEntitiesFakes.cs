// 1. Фейковая реализация PhiladelphusRepository (из Infrastructure)
using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Tests.Domain.Fakes
{
    public class FakePhiladelphusRepository : Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepository
    {
        // Пустая реализация — для конструктора достаточно
    }

    // 2. Фейковая IDataStorageModel
    public class FakeDataStorage : IDataStorageModel
    {
        public Guid Uuid { get; set; } = Guid.CreateVersion7();
        public string Name { get; set; } = "TestStorage";
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public InfrastructureTypes InfrastructureType => throw new NotImplementedException();

        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories => throw new NotImplementedException();

        public IPhiladelphusRepositoriesInfrastructureRepository PhiladelphusRepositoriesInfrastructureRepository => throw new NotImplementedException();

        public IShrubMembersInfrastructureRepository ShrubMembersInfrastructureRepository => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsHidden { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTime LastCheckTime => throw new NotImplementedException();

        public bool HasPhiladelphusRepositoriesInfrastructureRepository => throw new NotImplementedException();

        public bool HasShrubMembersInfrastructureRepository => throw new NotImplementedException();

        public IReportsInfrastructureRepository ReportsInfrastructureRepository => throw new NotImplementedException();

        public bool HasReportsInfrastructureRepository => throw new NotImplementedException();

        public bool CheckAvailable()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckAvailableAsync()
        {
            throw new NotImplementedException();
        }

        public bool StartAvailableAutoChecking(int interval)
        {
            throw new NotImplementedException();
        }

        public bool StopAvailableAutoChecking()
        {
            throw new NotImplementedException();
        }
        // Добавь другие свойства по интерфейсу, если нужно
    }

    // Тесты для PhiladelphusRepositoryModel
    public class PhiladelphusRepositoryModelTests
    {
        [Fact]
        public void Constructor_ValidArgs_SetsOwnDataStorageAndContentShrub()
        {
            // Arrange
            var uuid = Guid.CreateVersion7();
            var dataStorage = new FakeDataStorage();
            var dbEntity = new FakePhiladelphusRepository();

            // Act
            var sut = new PhiladelphusRepositoryModelTestingFixture(uuid, dataStorage);

            // Assert
            sut.Uuid.Should().Be(uuid);
            sut.OwnDataStorage.Should().BeSameAs(dataStorage);
            sut.DataStorages.Should().Contain(dataStorage);
            sut.DataStorage.Should().Be(dataStorage);
            sut.ContentShrub.Should().NotBeNull();
            sut.Content.Should().HaveCount(1);
            sut.AliasesList.Should().NotBeNull();
            sut.State.Should().Be(State.Initialized);
        }

        [Fact]
        public void Constructor_NullDataStorage_ThrowsArgumentNullException()
        {
            // Arrange
            var act = () => new PhiladelphusRepositoryModelTestingFixture(Guid.CreateVersion7(), null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dataStorage");
        }

        // ... остальные тесты аналогично
    }

    internal class PhiladelphusRepositoryModelTestingFixture : PhiladelphusRepositoryModel
    {
        public PhiladelphusRepositoryModelTestingFixture(Guid uuid, IDataStorageModel dataStorage)
            : base(uuid, dataStorage) { }

        // Переопределяем абстрактный DataStorage
        public override Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages.IDataStorageModel DataStorage => OwnDataStorage;
    }
}