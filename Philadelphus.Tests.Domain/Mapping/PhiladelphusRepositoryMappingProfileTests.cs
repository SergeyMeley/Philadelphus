using AutoMapper;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Mapping.MainEntitiesMapping;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Philadelphus.Tests.Domain.Entities.MainEntities;

namespace Philadelphus.Tests.Domain.Mapping;

public class PhiladelphusRepositoryMappingProfileTests
{
    [Fact]
    public void Map_ModelToEntity_MapsAvailableAndDefaultDataStorages()
    {
        // Arrange
        var ownDataStorage = new FakeDataStorageModel();
        var shrubMembersDataStorage = new FakeDataStorageModel();
        var reportsDataStorage = new FakeDataStorageModel();
        var repository = new PhiladelphusRepositoryModelTestingFixture(
            Guid.CreateVersion7(),
            ownDataStorage);
        repository.DataStorages.Add(shrubMembersDataStorage);
        repository.DataStorages.Add(reportsDataStorage);
        repository.DefaultShrubMembersDataStorage = shrubMembersDataStorage;
        repository.DefaultReportsDataStorage = reportsDataStorage;
        var mapper = CreateMapper();

        // Act
        var result = mapper.Map<PhiladelphusRepository>(repository);

        // Assert
        result.AvailableDataStorageUuids.Should().BeEquivalentTo(new[]
        {
            ownDataStorage.Uuid,
            shrubMembersDataStorage.Uuid,
            reportsDataStorage.Uuid
        });
        result.DefaultDataStorageUuids[InfrastructureEntityGroups.ShrubMembers]
            .Should().Be(shrubMembersDataStorage.Uuid);
        result.DefaultDataStorageUuids[InfrastructureEntityGroups.Reports]
            .Should().Be(reportsDataStorage.Uuid);
    }

    [Fact]
    public void Map_EntityToModel_MapsAvailableAndDefaultDataStorages()
    {
        // Arrange
        var ownDataStorage = new FakeDataStorageModel();
        var shrubMembersDataStorage = new FakeDataStorageModel();
        var reportsDataStorage = new FakeDataStorageModel();
        var entity = new PhiladelphusRepository
        {
            Uuid = Guid.CreateVersion7(),
            OwnDataStorageUuid = ownDataStorage.Uuid,
            AvailableDataStorageUuids =
            [
                ownDataStorage.Uuid,
                shrubMembersDataStorage.Uuid,
                reportsDataStorage.Uuid
            ],
            DefaultDataStorageUuids = new()
            {
                [InfrastructureEntityGroups.ShrubMembers] = shrubMembersDataStorage.Uuid
            },
            ContentWorkingTreesUuids = Array.Empty<Guid>()
        };
        var dataStorages = new IDataStorageModel[]
        {
            ownDataStorage,
            shrubMembersDataStorage,
            reportsDataStorage
        };
        var mapper = CreateMapper();

        // Act
        var result = mapper.Map<PhiladelphusRepositoryModel>(entity, options =>
        {
            options.Items["DataStorages"] = dataStorages;
            options.Items[nameof(INotificationService)] = new FakeNotificationService();
            options.Items[nameof(IPropertiesPolicy<PhiladelphusRepositoryModel>)] =
                new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>();
            options.Items[nameof(IPropertiesPolicy<ShrubModel>)] =
                new EmptyPropertiesPolicy<ShrubModel>();
        });

        // Assert
        result.DataStorages.Should().BeEquivalentTo(dataStorages);
        result.DefaultShrubMembersDataStorage.Should().BeSameAs(shrubMembersDataStorage);
        result.DefaultReportsDataStorage.Should().BeNull();
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.AddProfile<AuditInfoMappingProfile>();
                cfg.AddProfile<PhiladelphusRepositoryMappingProfile>();
            },
            NullLoggerFactory.Instance);
        return configuration.CreateMapper();
    }
}
