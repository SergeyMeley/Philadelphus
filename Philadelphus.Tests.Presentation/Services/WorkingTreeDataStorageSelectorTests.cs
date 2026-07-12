using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Presentation.Services;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Tests.Common.Fakes.Entities;

namespace Philadelphus.Tests.Presentation.Services;

public class WorkingTreeDataStorageSelectorTests
{
    [Fact]
    public async Task SelectAsync_ValidDefault_ReturnsDefaultWithoutDialog()
    {
        var defaultStorage = CreateStorage(supportsShrubMembers: true);
        var dialogService = new FakeDataStorageSelectionDialogService();

        var result = await WorkingTreeDataStorageSelector.SelectAsync(
            defaultStorage,
            new[] { defaultStorage },
            dialogService);

        result.Should().BeSameAs(defaultStorage);
        dialogService.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task SelectAsync_WithoutDefault_ReturnsExplicitlySelectedSupportedStorage()
    {
        var unsupportedStorage = CreateStorage(supportsShrubMembers: false);
        var selectedStorage = CreateStorage(supportsShrubMembers: true);
        var dialogService = new FakeDataStorageSelectionDialogService(selectedStorage);

        var result = await WorkingTreeDataStorageSelector.SelectAsync(
            null,
            new[] { unsupportedStorage, selectedStorage },
            dialogService);

        result.Should().BeSameAs(selectedStorage);
        dialogService.OfferedDataStorages.Should().ContainSingle()
            .Which.Should().BeSameAs(selectedStorage);
    }

    [Fact]
    public async Task SelectAsync_CancelledDialog_ReturnsNull()
    {
        var availableStorage = CreateStorage(supportsShrubMembers: true);
        var dialogService = new FakeDataStorageSelectionDialogService();

        var result = await WorkingTreeDataStorageSelector.SelectAsync(
            null,
            new[] { availableStorage },
            dialogService);

        result.Should().BeNull();
        dialogService.CallCount.Should().Be(1);
    }

    private static FakeDataStorageModel CreateStorage(bool supportsShrubMembers)
        => new()
        {
            HasShrubMembersInfrastructureRepository = supportsShrubMembers
        };

    private sealed class FakeDataStorageSelectionDialogService : IDataStorageSelectionDialogService
    {
        private readonly IDataStorageModel? _selection;

        public FakeDataStorageSelectionDialogService(IDataStorageModel? selection = null)
        {
            _selection = selection;
        }

        public int CallCount { get; private set; }

        public IReadOnlyCollection<IDataStorageModel> OfferedDataStorages { get; private set; }
            = Array.Empty<IDataStorageModel>();

        public Task<IDataStorageModel?> SelectAsync(
            IEnumerable<IDataStorageModel> dataStorages,
            string message,
            string title = "Выбор хранилища")
        {
            CallCount++;
            OfferedDataStorages = dataStorages.ToList();
            return Task.FromResult(_selection);
        }
    }
}
