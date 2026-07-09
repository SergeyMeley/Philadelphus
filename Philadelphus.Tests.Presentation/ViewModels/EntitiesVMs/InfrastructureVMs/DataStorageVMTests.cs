using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Tests.Common.Fakes.Services;

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

    [Fact]
    public void IsHidden_SetTrueForMainDataStorage_DoesNotChangeModel()
    {
        // Arrange
        var model = new FakeDataStorageModel { IsMainDataStorage = true, IsHidden = false };
        var notificationService = new FakeNotificationService();
        using var sut = new DataStorageVM(model, notificationService);
        var changedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        // Act
        sut.IsHidden = true;

        // Assert
        model.IsHidden.Should().BeFalse();
        sut.IsHidden.Should().BeFalse();
        changedProperties.Should().Contain(nameof(DataStorageVM.IsHidden));
        changedProperties.Should().Contain(nameof(DataStorageVM.HeaderOpacity));
        notificationService.Messages.Should().ContainSingle("Настройки основного хранилища нельзя изменять.");
    }

    [Fact]
    public void IsDisabled_SetTrueForMainDataStorage_DoesNotChangeModel()
    {
        // Arrange
        var model = new FakeDataStorageModel { IsMainDataStorage = true, IsDisabled = false };
        var notificationService = new FakeNotificationService();
        using var sut = new DataStorageVM(model, notificationService);
        var changedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        // Act
        sut.IsDisabled = true;

        // Assert
        model.IsDisabled.Should().BeFalse();
        sut.IsDisabled.Should().BeFalse();
        changedProperties.Should().Contain(nameof(DataStorageVM.IsDisabled));
        notificationService.Messages.Should().ContainSingle("Настройки основного хранилища нельзя изменять.");
    }

    [Fact]
    public void Name_SetForMainDataStorage_DoesNotChangeModelAndShowsWarning()
    {
        // Arrange
        var model = new FakeDataStorageModel { Name = "Main storage", IsMainDataStorage = true };
        var notificationService = new FakeNotificationService();
        using var sut = new DataStorageVM(model, notificationService);
        var changedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        // Act
        sut.Name = "New storage";

        // Assert
        model.Name.Should().Be("Main storage");
        sut.Name.Should().Be("Main storage");
        changedProperties.Should().Contain(nameof(DataStorageVM.Name));
        notificationService.Messages.Should().ContainSingle("Настройки основного хранилища нельзя изменять.");
    }

    [Fact]
    public void IsMainDataStorage_ReturnsModelValue()
    {
        // Arrange
        var model = new FakeDataStorageModel { IsMainDataStorage = true };

        // Act
        using var sut = new DataStorageVM(model);

        // Assert
        sut.IsMainDataStorage.Should().BeTrue();
    }

    private sealed class FakeDataStorageModel : IDataStorageModel
    {
        public Guid Uuid { get; init; } = Guid.CreateVersion7();

        private string _name = "Test storage";

        public string Name
        {
            get => _name;
            set
            {
                if (IsMainDataStorage)
                    return;

                _name = value;
            }
        }

        private string _description = "Test storage";

        public string Description
        {
            get => _description;
            set
            {
                if (IsMainDataStorage)
                    return;

                _description = value;
            }
        }

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

        public bool IsMainDataStorage { get; init; }

        public bool IsAvailable => false;

        private bool _isDisabled;

        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (IsMainDataStorage)
                    return;

                _isDisabled = value;
            }
        }

        private bool _isHidden;

        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                if (IsMainDataStorage)
                    return;

                _isHidden = value;
            }
        }

        public DateTime LastCheckTime { get; } = DateTime.MinValue;

        public bool CheckAvailable() => false;

        public Task<bool> CheckAvailableAsync() => Task.FromResult(false);

        public bool StartAvailableAutoChecking(int interval) => false;

        public bool StopAvailableAutoChecking() => false;
    }
}
