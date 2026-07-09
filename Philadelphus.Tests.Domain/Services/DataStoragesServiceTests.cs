using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Mapping.InfrastructureEntitiesMapping;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;

namespace Philadelphus.Tests.Domain.Services;

public class DataStoragesServiceTests
{
    [Fact]
    public void GetStoragesModels_HiddenStorageWithNullDisabled_LoadsInfrastructureRepositories()
    {
        // Arrange
        var storageUuid = Guid.CreateVersion7();
        var sut = CreateSut(CreateDataStorage(
            storageUuid,
            isHidden: true,
            isDisabled: null,
            hasPhiladelphusRepository: true));

        var repository = CreatePhiladelphusRepository();
        var callsCount = 0;

        // Act
        var result = sut.GetStoragesModels((_, _, _) =>
        {
            callsCount++;
            return repository;
        }).ToList();

        // Assert
        result.Should().ContainSingle();
        result[0].IsHidden.Should().BeTrue();
        result[0].IsDisabled.Should().BeFalse();
        result[0].HasPhiladelphusRepositoriesInfrastructureRepository.Should().BeTrue();
        callsCount.Should().Be(1);
    }

    [Fact]
    public void GetStoragesModels_DisabledStorage_DoesNotLoadInfrastructureRepositories()
    {
        // Arrange
        var storageUuid = Guid.CreateVersion7();
        var sut = CreateSut(CreateDataStorage(
            storageUuid,
            isHidden: false,
            isDisabled: true,
            hasPhiladelphusRepository: true));

        var callsCount = 0;

        // Act
        var result = sut.GetStoragesModels((_, _, _) =>
        {
            callsCount++;
            throw new InvalidOperationException("Factory should not be called for disabled storages.");
        }).ToList();

        // Assert
        result.Should().ContainSingle();
        result[0].IsDisabled.Should().BeTrue();
        result[0].HasPhiladelphusRepositoriesInfrastructureRepository.Should().BeFalse();
        callsCount.Should().Be(0);
    }

    [Fact]
    public void GetStoragesModels_RepositoryFactoryThrows_ReturnsStorageAndSendsNotification()
    {
        // Arrange
        var storageUuid = Guid.CreateVersion7();
        var notificationService = new FakeNotificationService();
        var sut = CreateSut(
            CreateDataStorage(storageUuid, isHidden: false, isDisabled: false, hasPhiladelphusRepository: true),
            notificationService);

        // Act
        var act = () => sut.GetStoragesModels((_, _, _) =>
            throw new InvalidOperationException("Infrastructure repository is unavailable.")).ToList();

        // Assert
        var result = act.Should().NotThrow().Subject;
        result.Should().ContainSingle();
        result[0].HasPhiladelphusRepositoriesInfrastructureRepository.Should().BeFalse();
        notificationService.Messages.Should().ContainSingle(x => x.Contains(InfrastructureEntityGroups.PhiladelphusRepositories.ToString()));
    }

    private static DataStoragesService CreateSut(
        DataStorage dataStorage,
        FakeNotificationService? notificationService = null)
    {
        var mapperConfiguration = new MapperConfiguration(
            cfg => cfg.AddProfile<DataStorageMappingProfile>(),
            NullLoggerFactory.Instance);
        var mapper = mapperConfiguration.CreateMapper();
        var logger = Mock.Of<ILogger>();

        var connectionStrings = new ConnectionStringsCollectionConfig
        {
            ConnectionStringsContainers = new List<ConnectionStringsContainer>
            {
                new()
                {
                    StorageUuid = dataStorage.Uuid,
                    InfrastructureType = dataStorage.InfrastructureType,
                    ProviderName = "Test",
                    ConnectionStrings = new Dictionary<InfrastructureEntityGroups, string>()
                }
            }
        };

        var dataStorages = new DataStoragesCollectionConfig
        {
            DataStorages = new List<DataStorage> { dataStorage }
        };

        return new DataStoragesService(
            mapper,
            logger,
            notificationService ?? new FakeNotificationService(),
            new FakeUserService(),
            Options.Create(new ApplicationSettingsConfig()),
            Options.Create(connectionStrings),
            Options.Create(dataStorages));
    }

    private static DataStorage CreateDataStorage(
        Guid uuid,
        bool isHidden,
        bool? isDisabled,
        bool hasPhiladelphusRepository)
    {
        return new DataStorage
        {
            Uuid = uuid,
            Name = "Test storage",
            Description = "Test storage",
            InfrastructureType = InfrastructureTypes.SQLiteEf,
            HasPhiladelphusRepositoriesInfrastructureRepository = hasPhiladelphusRepository,
            HasShrubMembersInfrastructureRepository = false,
            HasReportsInfrastructureRepository = false,
            IsHidden = isHidden,
            IsDisabled = isDisabled
        };
    }

    private static IPhiladelphusRepositoriesInfrastructureRepository CreatePhiladelphusRepository()
    {
        var repository = new Mock<IPhiladelphusRepositoriesInfrastructureRepository>();
        repository.Setup(x => x.EntityGroup).Returns(InfrastructureEntityGroups.PhiladelphusRepositories);
        repository.Setup(x => x.CheckAvailability()).Returns(true);
        return repository.Object;
    }
}
