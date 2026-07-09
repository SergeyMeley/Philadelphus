using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

namespace Philadelphus.Tests.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;

public class DataStorageVMTests
{
    [Fact]
    public void IsHidden_SetTrue_UpdatesModelAndHeaderOpacity()
    {
        // Arrange
        var model = new FakeDataStorageModel();
        using var sut = new DataStorageVM(model);
        var changedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        // Act
        sut.IsHidden = true;

        // Assert
        model.IsHidden.Should().BeTrue();
        sut.HeaderOpacity.Should().Be(0.55);
        changedProperties.Should().Contain(nameof(DataStorageVM.IsHidden));
        changedProperties.Should().Contain(nameof(DataStorageVM.HeaderOpacity));
    }

    [Fact]
    public void IsDisabled_SetTrue_DoesNotChangeIsHidden()
    {
        // Arrange
        var model = new FakeDataStorageModel { IsHidden = true, IsDisabled = false };
        using var sut = new DataStorageVM(model);

        // Act
        sut.IsDisabled = true;

        // Assert
        model.IsDisabled.Should().BeTrue();
        model.IsHidden.Should().BeTrue();
        sut.HeaderOpacity.Should().Be(0.55);
    }

    private sealed class FakeDataStorageModel : IDataStorageModel
    {
        public Guid Uuid { get; } = Guid.CreateVersion7();

        public string Name { get; set; } = "Test storage";

        public string Description { get; set; } = "Test storage";

        public InfrastructureTypes InfrastructureType { get; } = InfrastructureTypes.SQLiteEf;

        public string ProviderName { get; set; } = string.Empty;

        public Dictionary<InfrastructureEntityGroups, string> ConnectionStrings { get; } = new();

        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories { get; } = new();

        public IPhiladelphusRepositoriesInfrastructureRepository PhiladelphusRepositoriesInfrastructureRepository => null!;

        public IShrubMembersInfrastructureRepository ShrubMembersInfrastructureRepository => null!;

        public IReportsInfrastructureRepository ReportsInfrastructureRepository => null!;

        public bool HasPhiladelphusRepositoriesInfrastructureRepository => false;

        public bool HasShrubMembersInfrastructureRepository => false;

        public bool HasReportsInfrastructureRepository => false;

        public bool IsAvailable => false;

        public bool IsDisabled { get; set; }

        public bool IsHidden { get; set; }

        public DateTime LastCheckTime { get; } = DateTime.MinValue;

        public bool CheckAvailable() => false;

        public Task<bool> CheckAvailableAsync() => Task.FromResult(false);

        public bool StartAvailableAutoChecking(int interval) => false;

        public bool StopAvailableAutoChecking() => false;
    }
}
